using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Contracts;
using SecEdgarMiner.Data.Entities;
using SecEdgarMiner.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Activity
{
    public class GetLatestForm4Filings
    {
        public GetLatestForm4Filings(IForm4RssWorker form4Worker, ILogger<GetLatestForm4Filings> logger)
        {
            _form4RssWorker = form4Worker;
            _logger = logger;
        }

        private readonly IForm4RssWorker _form4RssWorker;
        private readonly ILogger<GetLatestForm4Filings> _logger;

        [FunctionName(nameof(GetLatestForm4Filings))]
        public async Task<Form4FilingsModel> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var fetchCount = 100;

            var form4RssUri = $"https://www.sec.gov/cgi-bin/browse-edgar?action=getcurrent&CIK=&type=4&company=&dateb=&owner=only&start=0&count={fetchCount}&output=atom";

            var form4RssFeedArgs = new Form4RssFeedArgs() { State = context.GetInput<RssFeed>() };

            var feed = await _form4RssWorker.GetRssFeedAsync(form4RssUri, form4RssFeedArgs);
            var form4InfoList = await _form4RssWorker.GetForm4InfoListAsync(feed);

            var form4Filings = new Form4FilingsModel()
            {
                FeedState = form4RssFeedArgs.State,
                Filings = form4InfoList.ToList()
            };

            if (form4RssFeedArgs.DistinctFilingsOnly)
            {
                var uniqueForm4InfoList = new List<Form4InfoModel>();

                foreach (var form4Info in form4InfoList)
                {
                    if (!uniqueForm4InfoList.Any(uniqueForm4 => uniqueForm4.XmlUrl == form4Info.XmlUrl))
                    {
                        uniqueForm4InfoList.Add(form4Info);
                    }
                }

                _logger.LogInformation($"Form4 filings Count(Distinct): {uniqueForm4InfoList.Count()}");

                form4Filings.Filings = uniqueForm4InfoList;
            }

            _logger.LogInformation($"Form4 filings Count(): {form4InfoList.Count()}");

            return form4Filings;
        }
    }
}
