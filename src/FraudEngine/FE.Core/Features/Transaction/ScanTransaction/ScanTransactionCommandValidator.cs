using FastEndpoints;
using FluentValidation;

namespace FE.Core.Features.Transaction.ScanTransaction
{
    public class ScanTransactionCommandValidator : Validator<ScanTransactionCommand>
    {
        public ScanTransactionCommandValidator()
        {
            RuleFor(x => x.ReferenceId)
                .NotEmpty()
                .WithMessage("Reference ID is required.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                .WithMessage("Account number is required.");

            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithMessage("Customer name is required.");

            RuleFor(x => x.Amount)
                .NotEmpty()
                .WithMessage("Amount is required.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.")
                .MaximumLength(3)
                .WithMessage("Currency must be a 3-letter ISO code. e.g. USD, ZAR");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Country is required.")
                .MaximumLength(2)
                .WithMessage("Country must be a 2-letter ISO code. e.g. US, ZA");

            RuleFor(x => x.BeneficiaryAccountNumber)
                .NotEmpty()
                .When(y => y.MerchantId is null && y.MerchantName is null)
                .WithMessage("Beneficiary account number is required when merchant information is not provided.");

            RuleFor(x => x.MerchantId)
                .NotEmpty()
                .When(y => y.BeneficiaryAccountNumber is null)
                .WithMessage("Merchant ID is required when beneficiary account number is not provided.");

            RuleFor(x => x.MerchantName)
                .NotEmpty()
                .When(y => y.MerchantId is not null)
                .WithMessage("Merchant name is required when merchant ID is provided.");

            RuleFor(x => x.PaymentChannel)
                .NotNull()
                .WithMessage("Payment channel is required.")
                .Must(x => Enum.IsDefined(x))
                .WithMessage("Invalid payment channel.");

            RuleFor(x => x.PaymentTiming)
                .NotNull()
                .WithMessage("Payment timing is required.")
                .Must(x => Enum.IsDefined(x))
                .WithMessage("Invalid payment timing.");
        }
    }
}
