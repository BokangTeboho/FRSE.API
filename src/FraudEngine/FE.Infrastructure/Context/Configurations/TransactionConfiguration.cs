using FE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FE.Infrastructure.Context.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.Currency).HasMaxLength(3);
            builder.Property(t => t.Country).HasMaxLength(3);
            builder.Property(t => t.AccountNumber).HasMaxLength(50);
            builder.Property(t => t.MerchantId).HasMaxLength(50);
            builder.Property(t => t.BeneficiaryAccountNumber).HasMaxLength(50);
            builder.Property(t => t.PaymentTiming).HasConversion<string>();
            builder.Property(t => t.PaymentChannel).HasConversion<string>();

            builder.HasIndex(t => new { t.AccountNumber, t.CreatedAt });
            builder.HasIndex(t => new { t.ReferenceId, t.AccountNumber }).IsUnique();
        }
    }
}
