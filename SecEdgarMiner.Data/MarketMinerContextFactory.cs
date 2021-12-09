using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SecEdgarMiner.Common;

namespace SecEdgarMiner.Data
{
    public class MarketMinerContextFactory : IDesignTimeDbContextFactory<MarketMinerContext>
    {
        public MarketMinerContext CreateDbContext(string[] args)
        {
            // this method is only used during local dev
            // it is not used by the app during runtime

            var keyVaultUri = "https://secedgarminerkeyvault.vault.azure.net/";

            var connectionString = KeyVaultHelper.GetSecretValueAsync(keyVaultUri, "ConnectionStrings--SqlConnectionString").Result;

            var optionsBuilder = new DbContextOptionsBuilder<MarketMinerContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new MarketMinerContext(optionsBuilder.Options);
        }
    }
}
