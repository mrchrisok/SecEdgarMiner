using Microsoft.EntityFrameworkCore;
using SecEdgarMiner.Data.Entities;
using System.Threading.Tasks;

namespace SecEdgarMiner.Data
{
    public class MarketMinerContext : DbContext
    {
        public MarketMinerContext(DbContextOptions<MarketMinerContext> options) : base(options)
        {
        }

        public DbSet<Form4Info> Form4Infos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Form4Info>()
               .Property(e => e.PeriodOfReport).IsRequired();
            builder.Entity<Form4Info>()
               .Property(e => e.OwnerName).IsRequired().HasMaxLength(100);
            builder.Entity<Form4Info>()
               .Property(e => e.OwnerCik).IsRequired().HasMaxLength(100);
            builder.Entity<Form4Info>()
               .Property(e => e.OwnerOfficerTitle).HasMaxLength(100);

            builder.Entity<Form4Info>()
               .Property(e => e.IssuerName).IsRequired().HasMaxLength(100);
            builder.Entity<Form4Info>()
               .Property(e => e.IssuerTradingSymbol).IsRequired().HasMaxLength(20);
            builder.Entity<Form4Info>()
               .Property(e => e.IssuerCik).IsRequired().HasMaxLength(100);

            builder.Entity<Form4Info>()
                    .Property(e => e.PurchaseTransactionsTotal).HasColumnType("decimal(18,2)");

            builder.Entity<Form4Info>()
            .Property(e => e.HtmlUrl).IsRequired().HasMaxLength(500);
            builder.Entity<Form4Info>()
               .Property(e => e.XmlUrl).IsRequired().HasMaxLength(500);

            builder.Entity<Form4Info>()
               .Property(e => e.DateCreated)
                  .HasDefaultValueSql("getutcdate()");


            builder.Entity<Form4Info>()
               .ToTable(nameof(Form4Info))
               .HasIndex(e => e.HtmlUrl).IsUnique();
        }

        public async Task<long> CreateAsync(Form4Info entity)
        {
            var entry = await AddAsync(entity);
            await SaveChangesAsync();

            return entry.Entity.Id.Value;
        }

        public async Task<IEntity> GetAsync(long entityId)
        {
            var entry = await FindAsync(typeof(Form4Info), entityId);
            return entry as Form4Info;
        }

        public async Task UpdateAsync(Form4Info entity)
        {
            if (entity.Id.HasValue)
            {
                Update(entity);
            }
            else
            {
                await CreateAsync(entity);
            }

            await SaveChangesAsync();
        }

        public async Task DeleteAsync(Form4Info entity)
        {
            Remove(entity);
            await SaveChangesAsync();
        }
    }
}
