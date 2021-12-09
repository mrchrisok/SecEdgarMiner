using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Contracts
{
    public interface IRssWorker
    {
        Task<SyndicationFeed> GetRssFeedAsync(string rssUrl, IRssFeedArgs args);
        Task<IEnumerable<SyndicationItem>> GetRssFeedItemsAsync(string rssUrl, IRssFeedArgs args);
        Task ProcessRssFeedAsync(SyndicationFeed feed);
        Task ProcessRssItemAsync(SyndicationItem rssItem);
    }
}