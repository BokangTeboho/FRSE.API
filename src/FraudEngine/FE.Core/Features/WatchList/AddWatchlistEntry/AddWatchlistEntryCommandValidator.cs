using FastEndpoints;
using FluentValidation;

namespace FE.Core.Features.WatchList.AddWatchlistEntry
{
    public class AddWatchlistEntryCommandValidator : Validator<AddWatchlistEntryCommand>
    {
        public AddWatchlistEntryCommandValidator()
        {
            RuleFor(x => x.EntityType)
                .Must(e => Enum.IsDefined(e))
                .WithMessage("Invalid entity type.");

            RuleFor(x => x.EntityIdentifier)
                .NotEmpty()
                .WithMessage("Entity identifier is required.");

            RuleFor(x => x.RiskLevel)
                .Must(r => Enum.IsDefined(r))
                .WithMessage("Invalid risk level.");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reason is required.");
        }
    }
}
