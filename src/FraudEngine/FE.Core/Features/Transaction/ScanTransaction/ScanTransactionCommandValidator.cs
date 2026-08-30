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

            RuleFor(x => x.Amount)
                .NotEmpty()
                .WithMessage("Amount is required.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .WithMessage("Currency is required.");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Country is required.");

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
