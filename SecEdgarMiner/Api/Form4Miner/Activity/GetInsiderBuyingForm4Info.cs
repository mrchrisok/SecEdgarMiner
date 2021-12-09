using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SecEdgarMiner.Contracts;
using SecEdgarMiner.Domain.Models;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Activity
{
    public class GetInsiderBuyingForm4Info
    {
        public GetInsiderBuyingForm4Info(IForm4Engine form4Engine, ILogger<Form4MinerTimer> logger)
        {
            _form4Engine = form4Engine;
            _logger = logger;
        }

        private readonly IForm4Engine _form4Engine;
        private readonly ILogger<Form4MinerTimer> _logger;

        [FunctionName(nameof(GetInsiderBuyingForm4Info))]
        public async Task<Form4InfoModel> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var form4Info = context.GetInput<Form4InfoModel>();
            var insiderBuyingForm4Info = await _form4Engine.GetInsiderBuyingForm4InfoAsync(form4Info);
            return insiderBuyingForm4Info;
        }
    }
}
