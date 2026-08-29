using FE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FE.Infrastructure.Context.Configurations
{
    public class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
    {
        public void Configure(EntityTypeBuilder<FraudAlert> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Severity).HasConversion<string>();

            builder.HasIndex(a => a.TransactionId);
        }
    }
}