using FE.Core.Interfaces;
using FE.Infrastructure.Context;
using FE.Infrastructure.Repositories;
using FE.Infrastructure.Rules;
using FE.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FE.API.ConfigurationExtensions
{
    public static class ServicesExtensions
    {
        public static void AddDbContext(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddDbContext<FraudEngineDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("FraudEngine")));
        }

        public static void ConfigureRulesOptions(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<ThresholdRuleOptions>(configuration.GetSection("FraudRules:Threshold"));
            services.Configure<VelocityRuleOptions>(configuration.GetSection("FraudRules:Velocity"));
            services.Configure<StructuringRuleOptions>(configuration.GetSection("FraudRules:Structuring"));
            services.Configure<BehavioralDeviationRuleOptions>(configuration.GetSection("FraudRules:BehavioralDeviation"));
            services.Configure<GeographicRuleOptions>(configuration.GetSection("FraudRules:GeographicRule"));
            services.Configure<WatchlistCacheOptions>(configuration.GetSection("FraudRules:WatchlistCache"));
        }

        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IFraudAlertRepository, FraudAlertRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void RegisterRules(this IServiceCollection services)
        {
            services.AddSingleton<IFraudRule, ThresholdRule>();
            services.AddSingleton<IFraudRule, WatchlistRule>();
            services.AddSingleton<IFraudRule, VelocityRule>();
            services.AddSingleton<IFraudRule, GeographicRule>();
            services.AddSingleton<IFraudRule, UnknownCountryRule>();
            services.AddSingleton<IFraudRule, BehavioralDeviationRule>();
            services.AddSingleton<IFraudRule, StructuringRule>();
        }
    }
}
