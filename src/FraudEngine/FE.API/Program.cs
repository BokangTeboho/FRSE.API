using FastEndpoints;
using FastEndpoints.Swagger;
using FE.API.ConfigurationExtensions;
using FE.API.Middleware;
using FE.Core.Features.Transaction.ScanTransaction;
using FE.Core.Interfaces;
using FE.Infrastructure.Resilience;
using FE.Infrastructure.Services;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.SwaggerDocument();
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
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.UseFastEndpoints(c => c.Serializer.Options.Converters.Add(new JsonStringEnumConverter()));
app.UseSwaggerGen();
app.MapHealthChecks("/health");
app.Run();
