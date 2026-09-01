using System.Security.Claims;
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
            var userId = User.FindFirstValue("sub");
            
            if (userId is null)
            {
                _logger.LogInformation($"{User?.Identity?.Name} is here");
                _logger.LogInformation($"{String.Join(", ", User?.Claims.Select(x => x.Value))} is here");
                _logger.LogWarning("Unauthorized attempt to add watchlist entry.");
                HttpContext.Response.StatusCode = 401;
                return;
            }

            req.ModifiedByIdentifier = userId;

            var result = await req.ExecuteAsync(ct);

            _logger.LogInformation(
                "Watchlist entry added: EntityType={EntityType}, Identifier={Identifier}",
                result.EntityType, result.EntityIdentifier);

            await Send.OkAsync(result, ct);
        }
    }
}
