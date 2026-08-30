using FastEndpoints;
using FE.Core.Interfaces;

namespace FE.Core.Features.Alerts.SearchAlerts
{
    public class SearchAlertsCommandHandler(IFraudAlertRepository alertRepository)
        : ICommandHandler<SearchAlertsCommand, SearchAlertsResult>
    {
        public async Task<SearchAlertsResult> ExecuteAsync(SearchAlertsCommand command, CancellationToken ct)
        {
            var (alerts, totalCount) = await alertRepository.Search(
                command.TransactionIds,
                command.Severities,
                command.RuleNames,
                command.Page,
                command.PageSize,
                ct);

            var items = alerts.Select(a => new AlertItem(
                a.Id,
                a.TransactionId,
                a.RuleName,
                a.Severity,
                a.Description,
                a.CreatedAt)).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / command.PageSize);

            return new SearchAlertsResult(items, command.Page, command.PageSize, totalCount, totalPages);
        }
    }
}
