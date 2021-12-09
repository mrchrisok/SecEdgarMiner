using SecEdgarMiner.Data.Entities;

namespace SecEdgarMiner.Contracts
{
    public interface IRssFeedArgs
    {
        bool DistinctFilingsOnly { get; }
        bool LatestItemsOnly { get; }
        RssFeed State { get; set; }
    }
}
