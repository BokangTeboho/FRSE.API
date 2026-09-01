using FastEndpoints;
using FastEndpoints.Swagger;
using FE.API.ConfigurationExtensions;
using FE.API.Middleware;
using FE.API.Swagger;
using FE.Core.Features.Transaction.ScanTransaction;
using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using FE.Infrastructure.Resilience;
using FE.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NSwag;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
var keycloakAuthority = builder.Configuration["Keycloak:Authority"]!;
var swaggerClientId = builder.Configuration["Keycloak:SwaggerClientId"]!;

builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Fraud Engine API";
        s.Version = "v1";

        if (builder.Environment.IsDevelopment())
        {
            s.AddAuth("KeycloakOAuth", new()
            {
                Type = OpenApiSecuritySchemeType.OAuth2,
                Description = "Login via Keycloak",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = $"{keycloakAuthority}/protocol/openid-connect/auth",
                        TokenUrl = $"{keycloakAuthority}/protocol/openid-connect/token",
                        Scopes = new Dictionary<string, string>()
                    }
                }
            });
        }

        s.SchemaSettings.SchemaProcessors.Add(new EnumSchemaFilter());

        s.AddAuth("Bearer", new()
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste a valid JWT token directly"
        });
    };
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddFastEndpoints(options =>
{
    options.Assemblies =
    [
        typeof(ScanTransactionCommandHandler).Assembly
    ];
    options.IncludeAbstractValidators = true;
});

builder.Services.AddKeycloakAuth(builder.Configuration);
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.ConfigureRulesOptions(builder.Configuration);
builder.Services.RegisterRepositories();
builder.Services.RegisterRules();
builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddDatabaseResilience();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = builder.Configuration
        .GetValue<int>("FraudRules:WatchlistCache:SizeLimit");
});

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c => c.Serializer.Options.Converters.Add(new JsonStringEnumConverter()));
app.UseSwaggerGen(uiConfig: s =>
{
    if (app.Environment.IsDevelopment())
    {
        s.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings
        {
            ClientId = swaggerClientId,
            UsePkceWithAuthorizationCodeGrant = true
        };
    }
});
app.MapHealthChecks("/health");
app.Run();
