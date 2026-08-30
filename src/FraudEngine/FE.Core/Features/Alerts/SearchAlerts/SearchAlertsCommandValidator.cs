using FastEndpoints;
using FluentValidation;

namespace FE.Core.Features.Alerts.SearchAlerts
{
    public class SearchAlertsCommandValidator : Validator<SearchAlertsCommand>
    {
        public SearchAlertsCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => x.TransactionIds?.Count > 0
                    || x.Severities?.Count > 0
                    || x.RuleNames?.Count > 0)
                .WithMessage("At least one search filter must be provided.");

            RuleForEach(x => x.Severities)
                .Must(s => Enum.IsDefined(s))
                .WithMessage("Invalid severity value.");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
