using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.Retry;

namespace FE.Infrastructure.Resilience;

public static class ResilienceExtensions
{
    public const string DatabasePipelineName = "database-retry";

    public static IServiceCollection AddDatabaseResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(DatabasePipelineName, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<DbUpdateException>()
                    .Handle<NpgsqlException>(ex => ex.IsTransient)
                    .Handle<TimeoutException>()
            });
        });

        return services;
    }
}
