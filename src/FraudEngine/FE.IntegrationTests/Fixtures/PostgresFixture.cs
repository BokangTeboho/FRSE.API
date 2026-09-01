using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using FE.Infrastructure.Repositories;
using FE.Infrastructure.Resilience;
using FE.Infrastructure.Rules;
using FE.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace FE.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private ServiceProvider _serviceProvider = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();

        services.AddDbContext<FraudEngineDbContext>(options =>
            options.UseNpgsql(_container.GetConnectionString()));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFraudAlertRepository, FraudAlertRepository>();

        services.AddDatabaseResilience();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IOptions<ThresholdRuleOptions>>(Options.Create(
            new ThresholdRuleOptions
            {
                DefaultLimit = 10_000m,
                Limits = new() { ["USD"] = 10_000m, ["ZAR"] = 150_000m }
            }));
        services.AddSingleton<IOptions<VelocityRuleOptions>>(Options.Create(
            new VelocityRuleOptions
            {
                Window = TimeSpan.FromMinutes(10),
                MaxTransactions = 5
            }));
        services.AddSingleton<IOptions<GeographicRuleOptions>>(Options.Create(
            new GeographicRuleOptions
            {
                MinTimeBetweenCountries = TimeSpan.FromHours(2)
            }));
        services.AddSingleton<IOptions<StructuringRuleOptions>>(Options.Create(
            new StructuringRuleOptions
            {
                DefaultThreshold = 10_000m,
                Thresholds = new() { ["USD"] = 10_000m, ["ZAR"] = 25_000m },
                ProximityPercentage = 0.1m
            }));
        services.AddSingleton<IOptions<BehavioralDeviationRuleOptions>>(Options.Create(
            new BehavioralDeviationRuleOptions { DeviationMultiplier = 3.0m }));

        services.AddSingleton<IFraudRule, ThresholdRule>();
        services.AddSingleton<IFraudRule, VelocityRule>();
        services.AddSingleton<IFraudRule, GeographicRule>();
        services.AddSingleton<IFraudRule, StructuringRule>();
        services.AddSingleton<IFraudRule, UnknownCountryRule>();
        services.AddSingleton<IFraudRule, WatchlistRule>();
        services.AddSingleton<IFraudRule, BehavioralDeviationRule>();

        services.AddMemoryCache();
        services.AddSingleton<IOptions<WatchlistCacheOptions>>(Options.Create(
            new WatchlistCacheOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30),
                NegativeCacheDuration = TimeSpan.FromMinutes(5)
            }));
        services.AddScoped<IWatchlistService, WatchlistService>();

        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FraudEngineDbContext>();
        await db.Database.MigrateAsync();
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _container.DisposeAsync();
    }
}
