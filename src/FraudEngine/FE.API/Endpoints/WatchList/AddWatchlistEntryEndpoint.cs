using System.Security.Claims;
using FastEndpoints;
using FE.Core.Features.WatchList.AddWatchlistEntry;

namespace FE.API.Endpoints.WatchList;

public class AddWatchlistEntryEndpoint(ILogger<AddWatchlistEntryEndpoint> logger)
    : Endpoint<AddWatchlistEntryCommand, AddWatchlistEntryResult>
{
    public override void Configure()
    {
        Post("/watchlist/entry");
    }

    public override async Task HandleAsync(AddWatchlistEntryCommand req, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
            logger.LogWarning("sub claim missing from token; ModifiedByIdentifier will be empty.");
        else
            req.ModifiedByIdentifier = userId;

        var result = await req.ExecuteAsync(ct);

        logger.LogInformation(
            "Watchlist entry added: EntityType={EntityType}, Identifier={Identifier}",
            result.EntityType, result.EntityIdentifier);

        await Send.OkAsync(result, ct);
    }
}
