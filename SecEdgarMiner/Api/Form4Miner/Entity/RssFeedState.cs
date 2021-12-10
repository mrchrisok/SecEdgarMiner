using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SecEdgarMiner.Contracts;
using System;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RssFeedState : IRssFeedState
    {
        public RssFeedState() { }
        public RssFeedState(Data.Entities.RssFeed state) { State = state; }

        [JsonProperty]
        private Data.Entities.RssFeed State { get; set; }

        [FunctionName(nameof(RssFeedState))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            if (!context.HasState)
            {
                var feed = new Data.Entities.RssFeed() { LastUpdatedTime = DateTimeOffset.UtcNow.AddDays(-1) };
                var state = new RssFeedState(feed);
                context.SetState(state);
            }

            return context.DispatchAsync<RssFeedState>();
        }

        public Task<Data.Entities.RssFeed> GetAsync()
        {
            return Task.FromResult(State);
        }

        public Task UpdateAsync(Data.Entities.RssFeed state)
        {
            State = state;

            return Task.CompletedTask;
        }
    }
}