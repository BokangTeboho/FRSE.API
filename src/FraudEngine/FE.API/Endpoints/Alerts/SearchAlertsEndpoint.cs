using FastEndpoints;
using FE.Core.Features.Alerts.SearchAlerts;

namespace FE.API.Endpoints.Alerts
{
    public class SearchAlertsEndpoint : Endpoint<SearchAlertsCommand, SearchAlertsResult>
    {
        public override void Configure()
        {
            Post("/alerts/search");
            AllowAnonymous();
        }

        public override async Task HandleAsync(SearchAlertsCommand req, CancellationToken ct)
        {
            var result = await req.ExecuteAsync(ct);
            await Send.OkAsync(result, ct);
        }
    }
}
