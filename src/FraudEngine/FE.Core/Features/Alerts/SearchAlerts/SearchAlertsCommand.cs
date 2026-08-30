using FastEndpoints;
using FE.Core.Enums;

namespace FE.Core.Features.Alerts.SearchAlerts
{
    public record SearchAlertsCommand : ICommand<SearchAlertsResult>
    {
        public List<Guid>? TransactionIds { get; init; }
        public List<Severity>? Severities { get; init; }
        public List<string>? RuleNames { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    public record SearchAlertsResult(
        IReadOnlyList<AlertItem> Alerts,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);

    public record AlertItem(
        Guid Id,
        Guid TransactionId,
        string RuleName,
        Severity Severity,
        string Description,
        DateTimeOffset CreatedAt);
}
