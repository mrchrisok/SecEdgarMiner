using Microsoft.EntityFrameworkCore;
using SecuritiesExchangeCommission.Edgar;

namespace SecEdgarMiner.Data
{
   public class MarketMinerDbContext : DbContext
   {
	  public MarketMinerDbContext(DbContextOptions<MarketMinerDbContext> options) : base(options)
	  {
	  }
	  public DbSet<StatementOfBeneficialOwnership> StatementsOfBeneficialOwnership { get; set; }
   }
}
