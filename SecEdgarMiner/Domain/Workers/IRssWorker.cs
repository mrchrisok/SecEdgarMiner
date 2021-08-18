using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Domain.Workers
{
   public interface IRssWorker
   {
	  Task<SyndicationFeed> GetRssFeedAsync(string rssUrl, bool latestItemsOnly = true);
	  Task<IEnumerable<SyndicationItem>> GetRssFeedItemsAsync(string rssUrl, bool latestItemsOnly = true);
	  Task ProcessRssFeedAsync(SyndicationFeed feed);
	  Task ProcessRssItemAsync(SyndicationItem rssItem);
   }
}