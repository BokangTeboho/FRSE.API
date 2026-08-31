using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FE.API.ConfigurationExtensions
{
    public static class AuthExtensions
    {
        public static void AddKeycloakAuth(this IServiceCollection services, ConfigurationManager configuration)
        {
            var keycloakSection = configuration.GetSection("Keycloak");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = keycloakSection["Authority"];
                    options.Audience = keycloakSection["Audience"];
                    options.RequireHttpsMetadata = keycloakSection.GetValue<bool>("RequireHttpsMetadata");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = keycloakSection["Authority"],
                        ValidAudience = keycloakSection["Audience"]
                    };
                });

            services.AddAuthorization();
        }
    }
}
