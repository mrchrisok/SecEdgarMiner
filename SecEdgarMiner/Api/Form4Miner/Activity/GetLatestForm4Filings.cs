using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Domain.Models;
using SecEdgarMiner.Domain.Workers;
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
	  public async Task<IEnumerable<Form4Info>> Run([ActivityTrigger] IDurableActivityContext context)
	  {
		 var fetchCount = 100;

		 var form4RssUri = $"https://www.sec.gov/cgi-bin/browse-edgar?action=getcurrent&CIK=&type=4&company=&dateb=&owner=only&start=0&count={fetchCount}&output=atom";

		 var feed = await _form4RssWorker.GetRssFeedAsync(form4RssUri, latestItemsOnly: true);
		 var form4InfoList = await _form4RssWorker.GetForm4InfoListAsync(feed);

		 var distinctFilingsOnly = context.GetInput<bool>();

		 if (distinctFilingsOnly)
		 {
			var uniqueForm4InfoList = new List<Form4Info>();

			foreach (var form4Info in form4InfoList)
			{
			   if (!uniqueForm4InfoList.Any(uniqueForm4 => uniqueForm4.XmlUrl == form4Info.XmlUrl))
			   {
				  uniqueForm4InfoList.Add(form4Info);
			   }
			}

			_logger.LogInformation($"Form4 filings Count(Distinct): {uniqueForm4InfoList.Count()}");

			return uniqueForm4InfoList;
		 }

		 _logger.LogInformation($"Form4 filings Count(): {form4InfoList.Count()}");

		 return form4InfoList;
	  }
   }
}
