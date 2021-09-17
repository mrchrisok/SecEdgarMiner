using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SecEdgarMiner.Common;

namespace SecEdgarMiner.Data
{
   public class MarketMinerContextFactory : IDesignTimeDbContextFactory<MarketMinerContext>
   {
	  public MarketMinerContext CreateDbContext(string[] args)
	  {
		 ConfigHelper.GetLocalSettings().TryGet("KeyVaultName", out string connectionString);

		 //var connectionString = KeyVaultHelper.GetSecretValueAsync(nameof(SecEdgarMiner), "ConnectionStrings:SqlConnectionString").Result;

		 var optionsBuilder = new DbContextOptionsBuilder<MarketMinerContext>();
		 optionsBuilder.UseSqlServer(connectionString);

		 return new MarketMinerContext(optionsBuilder.Options);
	  }
   }
}
