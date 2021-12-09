using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SecEdgarMiner.Contracts;
using System;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Entity
{
    public class RssFeedState : IRssFeedState
    {
        [FunctionName(nameof(RssFeedState))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            if (!context.HasState)
            {
                var feed = new Data.Entities.RssFeed() { LastUpdatedTime = DateTimeOffset.UtcNow.AddDays(-1) };
                var state = new RssFeedState() { State = feed };
                context.SetState(state);
            }

            return context.DispatchAsync<RssFeedState>();
        }

        public Data.Entities.RssFeed State { get; set; }

        public Task<Data.Entities.RssFeed> GetAsync()
        {
            return Task.FromResult(State);
        }

        public Task UpdateAsync(Data.Entities.RssFeed feed)
        {
            State = feed;

            return Task.CompletedTask;
        }
    }
}