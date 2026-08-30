using FastEndpoints;
using FE.Core.Features.Alerts.SearchAlerts;

namespace FE.API.Endpoints.Alerts
{
    public class SearchAlertsEndpoint : Endpoint<SearchAlertsCommand, SearchAlertsResult>
    {
        private readonly ILogger<SearchAlertsEndpoint> _logger;

        public SearchAlertsEndpoint(ILogger<SearchAlertsEndpoint> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/alerts/search");
            AllowAnonymous();
        }

        public override async Task HandleAsync(SearchAlertsCommand req, CancellationToken ct)
        {
            _logger.LogInformation(
                "Searching fraud alerts, page {Page}, pageSize {PageSize}",
                req.Page, req.PageSize);

            var result = await req.ExecuteAsync(ct);

            _logger.LogInformation(
                "Fraud alert search returned {TotalCount} result(s) across {TotalPages} page(s)",
                result.TotalCount, result.TotalPages);

            await Send.OkAsync(result, ct);
        }
    }
}
