using FastEndpoints;
using FE.Core.Features.WatchList.AddWatchlistEntry;

namespace FE.API.Endpoints.WatchList
{
    public class AddWatchlistEntryEndpoint : Endpoint<AddWatchlistEntryCommand, AddWatchlistEntryResult>
    {
        private readonly ILogger<AddWatchlistEntryEndpoint> _logger;

        public AddWatchlistEntryEndpoint(ILogger<AddWatchlistEntryEndpoint> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/watchlist/entry");
        }

        public override async Task HandleAsync(AddWatchlistEntryCommand req, CancellationToken ct)
        {
            var result = await req.ExecuteAsync(ct);

            _logger.LogInformation(
                "Watchlist entry added: EntityType={EntityType}, Identifier={Identifier}",
                result.EntityType, result.EntityIdentifier);

            await Send.OkAsync(result, ct);
        }
    }
}
