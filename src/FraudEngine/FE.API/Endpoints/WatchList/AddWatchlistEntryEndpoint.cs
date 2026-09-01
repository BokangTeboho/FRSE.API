using System.Security.Claims;
using FastEndpoints;
using FE.Core.Features.WatchList.AddWatchlistEntry;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(AddWatchlistEntryCommand req, CancellationToken ct)
        {
            req.ModifiedByIdentifier = User.FindFirstValue("sub")
                ?? throw new InvalidOperationException("User identifier claim is missing.");

            var result = await req.ExecuteAsync(ct);

            _logger.LogInformation(
                "Watchlist entry added: EntityType={EntityType}, Identifier={Identifier}",
                result.EntityType, result.EntityIdentifier);

            await Send.OkAsync(result, ct);
        }
    }
}
