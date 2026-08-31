using FastEndpoints;
using FE.Core.Interfaces;

namespace FE.API.Endpoints.WatchList
{
    public record GetWatchlistMerchantRequest
    {
        public required string Identifier { get; init; }
    }

    public class GetWatchlistMerchantEndpoint : Endpoint<GetWatchlistMerchantRequest, WatchlistEntryResponse>
    {
        private readonly IWatchlistService _watchlistService;
        private readonly ILogger<GetWatchlistMerchantEndpoint> _logger;

        public GetWatchlistMerchantEndpoint(
            IWatchlistService watchlistService,
            ILogger<GetWatchlistMerchantEndpoint> logger)
        {
            _watchlistService = watchlistService;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/watchlist/merchant/{Identifier}");
        }

        public override async Task HandleAsync(GetWatchlistMerchantRequest req, CancellationToken ct)
        {
            var entry = await _watchlistService.CheckMerchant(req.Identifier, ct);

            if (entry is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            _logger.LogInformation(
                "Watchlist merchant found: Identifier={Identifier}",
                req.Identifier);

            await Send.OkAsync(new WatchlistEntryResponse(
                entry.Id,
                entry.EntityType,
                entry.EntityIdentifier,
                entry.RiskLevel,
                entry.Reason,
                entry.AlertCount,
                entry.IsManualEntry,
                entry.AddedByIdentifier,
                entry.IsActive,
                entry.CreatedAt), ct);
        }
    }
}
