using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Api.Form4Miner.Activity;
using SecEdgarMiner.Api.Form4Miner.Entity;
using SecEdgarMiner.Contracts;
using SecEdgarMiner.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Orchestration
{
    public class GetInsiderBuying
    {
        public GetInsiderBuying(IForm4RssWorker form4Worker, ILogger<Form4MinerTimer> logger)
        {
            _form4RssWorker = form4Worker;
            _logger = logger;
        }

        private readonly IForm4RssWorker _form4RssWorker;
        private readonly ILogger<Form4MinerTimer> _logger;

        [FunctionName(nameof(GetInsiderBuying))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var form4RssFeedArgsId = new EntityId(nameof(RssFeedState), "SecEdgarMinerForm4RssFeed");

                using (await context.LockAsync(form4RssFeedArgsId))
                {
                    var form4RssFeedStateProxy = context.CreateEntityProxy<IRssFeedState>(form4RssFeedArgsId);
                    var form4RssFeedState = await form4RssFeedStateProxy.GetAsync();

                    var form4Filings = await context.CallActivityAsync<Form4FilingsModel>(nameof(GetLatestForm4Filings), form4RssFeedState);

                    await form4RssFeedStateProxy.UpdateAsync(form4Filings.FeedState);

                    if (form4Filings.Filings?.Count() == 0) return;
                    //

                    var parallelTasks = new List<Task<Form4InfoModel>>();

                    foreach (var form4Info in form4Filings.Filings)
                    {
                        var task = context.CallActivityAsync<Form4InfoModel>(nameof(GetInsiderBuyingForm4Info), form4Info);
                        parallelTasks.Add(task);
                    }

                    await Task.WhenAll(parallelTasks);

                    // aggregate & process tasks with resulst, if not null

                    var insiderBuyingForm4InfoList = parallelTasks.Where(task => task.Result != null).Select(task => task.Result);

                    _logger.LogInformation($"Form4 Insider Buys Count: {insiderBuyingForm4InfoList.Count()}");

                    if (insiderBuyingForm4InfoList.Count() > 0)
                    {
                        await context.CallActivityAsync(nameof(SendInsiderBuyingAlert), insiderBuyingForm4InfoList);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(GetInsiderBuying)} failed. Message: {ex.Message}");
            }
        }
    }
}
