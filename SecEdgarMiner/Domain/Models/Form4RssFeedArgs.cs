using SecEdgarMiner.Contracts;
using SecEdgarMiner.Data.Entities;

namespace SecEdgarMiner.Domain.Models
{
    public class Form4RssFeedArgs : IRssFeedArgs
    {
        public bool DistinctFilingsOnly => true;
        public bool LatestItemsOnly => true;
        public RssFeed State { get; set; }
    }
}
