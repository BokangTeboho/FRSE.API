using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FE.API.Services;

public class KeycloakUserInfoService(
    HttpClient httpClient,
    IOptionsMonitor<JwtBearerOptions> jwtOptions)
{
    public async Task<string?> GetUserSubAsync(string bearerToken, CancellationToken ct = default)
    {
        var userInfoEndpoint = jwtOptions
            .Get(JwtBearerDefaults.AuthenticationScheme)
            .Configuration?.UserInfoEndpoint;

        if (string.IsNullOrEmpty(userInfoEndpoint))
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, userInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        return doc.RootElement.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
    }
}
