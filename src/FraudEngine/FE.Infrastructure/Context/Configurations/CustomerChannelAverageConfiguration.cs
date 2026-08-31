using FE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FE.Infrastructure.Context.Configurations
{
    public class CustomerChannelAverageConfiguration : IEntityTypeConfiguration<CustomerChannelAverage>
    {
        public void Configure(EntityTypeBuilder<CustomerChannelAverage> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.AverageAmount).HasPrecision(18, 2);
            builder.Property(c => c.PaymentChannel).HasConversion<string>();

            builder.HasIndex(c => new { c.CustomerId, c.PaymentChannel }).IsUnique();
        }
    }
}
