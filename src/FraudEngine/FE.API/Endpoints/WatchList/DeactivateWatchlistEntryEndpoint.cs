using System.Security.Claims;
using FastEndpoints;
using FE.API.Services;
using FE.Core.Features.WatchList.DeactivateWatchlistEntry;

namespace FE.API.Endpoints.WatchList;

public class DeactivateWatchlistEntryEndpoint(
    ILogger<DeactivateWatchlistEntryEndpoint> logger,
    KeycloakUserInfoService keycloakUserInfo)
    : Endpoint<DeactivateWatchlistEntryCommand, DeactivateWatchlistEntryResult>
{
    public override void Configure()
    {
        Patch("/watchlist/{Id}/deactivate");
    }

    public override async Task HandleAsync(DeactivateWatchlistEntryCommand req, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub") ?? await GetUserSubFromKeycloakAsync(ct);

        if (userId is null)
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

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

    private async Task<string?> GetUserSubFromKeycloakAsync(CancellationToken ct)
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        return await keycloakUserInfo.GetUserSubAsync(authHeader["Bearer ".Length..], ct);
    }
}
