using FastEndpoints;
using FE.Core.Enums;
using FE.Core.Interfaces;

namespace FE.API.Endpoints.WatchList
{
    public record GetWatchlistBeneficiaryRequest
    {
        public required string Identifier { get; init; }
    }

    public record WatchlistEntryResponse(
        Guid Id,
        EntityType EntityType,
        string EntityIdentifier,
        Severity RiskLevel,
        string Reason,
        int AlertCount,
        bool IsManualEntry,
        string AddedByIdentifier,
        bool IsActive,
        DateTimeOffset CreatedAt);

    public class GetWatchlistBeneficiaryEndpoint : Endpoint<GetWatchlistBeneficiaryRequest, WatchlistEntryResponse>
    {
        private readonly IWatchlistService _watchlistService;
        private readonly ILogger<GetWatchlistBeneficiaryEndpoint> _logger;

        public GetWatchlistBeneficiaryEndpoint(
            IWatchlistService watchlistService,
            ILogger<GetWatchlistBeneficiaryEndpoint> logger)
        {
            _watchlistService = watchlistService;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/watchlist/beneficiary/{Identifier}");
        }

        public override async Task HandleAsync(GetWatchlistBeneficiaryRequest req, CancellationToken ct)
        {
            var entry = await _watchlistService.CheckBeneficiary(req.Identifier, ct);

            if (entry is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            _logger.LogInformation(
                "Watchlist beneficiary found: Identifier={Identifier}",
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
