using FE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FE.Infrastructure.Context
{
    public class FraudEngineDbContext(DbContextOptions<FraudEngineDbContext> options) : DbContext(options)
    {
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<WatchlistEntry> WatchlistEntries => Set<WatchlistEntry>();
        public DbSet<CustomerChannelAverage> CustomerChannelAverages => Set<CustomerChannelAverage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudEngineDbContext).Assembly);
        }
    }
}