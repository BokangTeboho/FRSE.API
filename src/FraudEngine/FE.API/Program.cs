using FastEndpoints;
using FastEndpoints.Swagger;
using FE.API.ConfigurationExtensions;
using FE.Core.Features.Transaction.ScanTransaction;
using FE.Core.Interfaces;
using FE.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .CreateLogger();

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.SwaggerDocument();
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

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = builder.Configuration
        .GetValue<int>("FraudRules:WatchlistCache:SizeLimit");
});

var app = builder.Build();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.MapHealthChecks("/health");
app.Run();
