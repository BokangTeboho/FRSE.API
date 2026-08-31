using FastEndpoints;
using FE.Core.Features.WatchList.DeactivateWatchlistEntry;

namespace FE.API.Endpoints.WatchList
{
    public class DeactivateWatchlistEntryEndpoint
        : Endpoint<DeactivateWatchlistEntryCommand, DeactivateWatchlistEntryResult>
    {
        private readonly ILogger<DeactivateWatchlistEntryEndpoint> _logger;

        public DeactivateWatchlistEntryEndpoint(ILogger<DeactivateWatchlistEntryEndpoint> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Patch("/watchlist/{Id}/deactivate");
        }

        public override async Task HandleAsync(DeactivateWatchlistEntryCommand req, CancellationToken ct)
        {
            var result = await req.ExecuteAsync(ct);

            if (result is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            _logger.LogInformation(
                "Watchlist entry deactivated: Id={Id}", result.Id);

            await Send.OkAsync(result, ct);
        }
    }
}
