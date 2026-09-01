using System.Security.Claims;
using FastEndpoints;
using FE.API.Services;
using FE.Core.Features.WatchList.AddWatchlistEntry;

namespace FE.API.Endpoints.WatchList;

public class AddWatchlistEntryEndpoint(
    ILogger<AddWatchlistEntryEndpoint> logger,
    KeycloakUserInfoService keycloakUserInfo)
    : Endpoint<AddWatchlistEntryCommand, AddWatchlistEntryResult>
{
    public override void Configure()
    {
        Post("/watchlist/entry");
    }

    public override async Task HandleAsync(AddWatchlistEntryCommand req, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub") ?? await GetUserSubFromKeycloakAsync(ct);

        if (userId is null)
        {
            HttpContext.Response.StatusCode = 401;
            return;
        }

        req.ModifiedByIdentifier = userId;

        var result = await req.ExecuteAsync(ct);

        logger.LogInformation(
            "Watchlist entry added: EntityType={EntityType}, Identifier={Identifier}",
            result.EntityType, result.EntityIdentifier);

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
