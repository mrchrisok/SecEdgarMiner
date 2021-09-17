using SecEdgarMiner.Data.Entities;
using System.Threading.Tasks;

namespace SecEdgarMiner.Data.Repositories
{
   public interface IRepository<T> where T : IEntity
   {
	  Task<long> CreateAsync(T entity);
	  Task<T> GetAsync(long entityId);
	  Task UpdateAsync(T entity);
	  Task DeleteAsync(T entity);
   }
}
