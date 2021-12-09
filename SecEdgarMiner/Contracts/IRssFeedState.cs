using System.Threading.Tasks;

namespace SecEdgarMiner.Contracts
{
    public interface IRssFeedState
    {
        Task<Data.Entities.RssFeed> GetAsync();
        Task UpdateAsync(Data.Entities.RssFeed state);
    }
}
