using System.Security.Claims;
using FastEndpoints;
using FE.Core.Features.WatchList.DeactivateWatchlistEntry;

namespace FE.API.Endpoints.WatchList;

public class DeactivateWatchlistEntryEndpoint(ILogger<DeactivateWatchlistEntryEndpoint> logger)
    : Endpoint<DeactivateWatchlistEntryCommand, DeactivateWatchlistEntryResult>
{
    public override void Configure()
    {
        Patch("/watchlist/{Id}/deactivate");
    }

    public override async Task HandleAsync(DeactivateWatchlistEntryCommand req, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId)) // to do
            logger.LogWarning("sub claim missing from token; ModifiedByIdentifier will be empty.");
        else
            req.ModifiedByIdentifier = userId;

        var result = await req.ExecuteAsync(ct);

        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        logger.LogInformation("Watchlist entry deactivated: Id={Id}", result.Id);

        await Send.OkAsync(result, ct);
    }
}
