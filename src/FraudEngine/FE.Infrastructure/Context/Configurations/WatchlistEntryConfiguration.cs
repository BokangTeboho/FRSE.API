using FE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FE.Infrastructure.Context.Configurations
{
    public class WatchlistEntryConfiguration : IEntityTypeConfiguration<WatchlistEntry>
    {
        public void Configure(EntityTypeBuilder<WatchlistEntry> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.EntityType).HasConversion<string>();
            builder.Property(w => w.RiskLevel).HasConversion<string>();

            builder.HasIndex(w => new { w.EntityType, w.EntityIdentifier });
        }
    }
}