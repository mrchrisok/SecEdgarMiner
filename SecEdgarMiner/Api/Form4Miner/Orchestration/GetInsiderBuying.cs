using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Api.Form4Miner.Activity;
using SecEdgarMiner.Domain.Models;
using SecEdgarMiner.Domain.Workers;
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
			var distinctFilingsOnly = true;

			var form4InfoList = await context.CallActivityAsync<IEnumerable<Form4Info>>(nameof(GetLatestForm4Filings), distinctFilingsOnly);

			if (form4InfoList?.Count() == 0) return;
			//

			var parallelTasks = new List<Task<Form4Info>>();

			foreach (var form4Info in form4InfoList)
			{
			   var task = context.CallActivityAsync<Form4Info>(nameof(GetInsiderBuyingForm4Info), form4Info);
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
		 catch (Exception ex)
		 {
			_logger.LogError($"GetInsiderBuying failed. Message: {ex.Message}");
		 }
	  }
   }
}
