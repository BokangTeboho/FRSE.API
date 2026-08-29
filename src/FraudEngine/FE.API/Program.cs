using FastEndpoints;
using FastEndpoints.Swagger;
using FE.API.ConfigurationExtensions;
using FE.Core.Features.Transaction.ScanTransaction;
using FE.Core.Interfaces;
using FE.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.SwaggerDocument();
builder.Services.AddFastEndpoints(o =>
{
    o.Assemblies =
    [
        typeof(ScanTransactionCommandHandler).Assembly
    ];
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.Run();
