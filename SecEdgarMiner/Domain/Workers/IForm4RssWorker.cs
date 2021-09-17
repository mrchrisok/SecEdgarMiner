using SecEdgarMiner.Domain.Models;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Domain.Workers
{
   public interface IForm4RssWorker : IRssWorker
   {
	  Task<IEnumerable<Form4InfoModel>> GetForm4InfoListAsync(SyndicationFeed feed);
	  Task<Form4InfoModel> GetForm4InfoAsync(SyndicationItem rssItem);
   }
}