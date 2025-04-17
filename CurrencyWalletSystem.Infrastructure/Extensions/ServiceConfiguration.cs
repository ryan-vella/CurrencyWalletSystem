using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Jobs;
using CurrencyWalletSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Diagnostics.CodeAnalysis;

namespace CurrencyWalletSystem.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddScoped<IExchangeRatePersister, ExchangeRatePersister>();
            services.AddScoped<IRawSqlExecutor, EfCoreRawSqlExecutor>();
            services.AddScoped<ISqlExecutor, SqlExecutor>();

            return services;
        }

        /// <summary>
        /// Configures Quartz with jobs and triggers.
        /// </summary>
        /// <param name="services">The service collection to add Quartz configuration.</param>
        public static IServiceCollection ConfigureQuartz(this IServiceCollection services)
        {
            services.AddQuartz(options =>
            {
                var jobKey = new JobKey("FetchAndStoreRates");

                options.AddJob<FetchAndStoreRatesJob>(opts => opts.WithIdentity(jobKey));

                options.AddTrigger(t => t
                    .ForJob(jobKey)
                    .WithSimpleSchedule(s => s.WithIntervalInMinutes(1).RepeatForever())
                );
            });

            services.AddQuartzHostedService();

            return services;
        }
    }

}
