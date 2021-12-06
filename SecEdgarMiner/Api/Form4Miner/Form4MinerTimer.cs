using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Api.Form4Miner.Orchestration;
using SecEdgarMiner.Domain.Engines;
using SecEdgarMiner.Domain.Workers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner
{
    public class Form4MinerTimer
    {
        public Form4MinerTimer(IForm4RssWorker form4RssWorker, IForm4Engine form4Engine, ILogger<Form4MinerTimer> logger)
        {
            _form4RssWorker = form4RssWorker;
            _form4Engine = form4Engine;
            _logger = logger;
        }

        private readonly IForm4RssWorker _form4RssWorker;
        private readonly IForm4Engine _form4Engine;
        private readonly ILogger<Form4MinerTimer> _logger;

        private static bool _appInitialized = false;

        [FunctionName(nameof(Form4MinerTimer))]
        public async Task TimerStart([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer,
           [DurableClient] IDurableOrchestrationClient starter)
        {
            if (_appInitialized && !InExecutionWindow())
            {
                return;
            }

            if (!_appInitialized)
            {
                _appInitialized = true;
            }

            string instanceId = await starter.StartNewAsync(nameof(GetInsiderBuying), null);

            _logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        private bool InExecutionWindow()
        {
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var easternTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

            if (new[] { DayOfWeek.Sunday, DayOfWeek.Saturday }.Contains(easternTimeNow.DayOfWeek))
            {
                return false;
            }

            // timer CRON expression is: 0 */5 * * * *
            // this means the function will fire every 5th minute of all hours

            if (easternTimeNow.TimeOfDay.Hours >= 5 && easternTimeNow.TimeOfDay.Hours < 20)
            {
                // IF between 5:00 AM and 8:00 PM
                // THEN continue to execution	
            }
            else
            {
                // IF NOT between 5:00 AM and 8:00 PM
                // AND the minute component of the current time is not a multiple of 20
                // THEN exit 
                if (easternTimeNow.Minute % 20 != 0) return false;
            }

            return true;
        }
    }
}