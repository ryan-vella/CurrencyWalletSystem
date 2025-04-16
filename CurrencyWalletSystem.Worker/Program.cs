using CurrencyWalletSystem.Gateway.Interfaces;
using CurrencyWalletSystem.Gateway.Services;
using CurrencyWalletSystem.Gateway.Settings;
using CurrencyWalletSystem.Infrastructure.Extensions;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        ConfigureServices(builder);

        await builder.RunConsoleAsync();
    }

    /// <summary>
    /// Configures services for the application, including dependencies for the worker, quartz jobs, and infrastructure.
    /// </summary>
    /// <param name="builder">The host builder to configure services.</param>
    private static void ConfigureServices(IHostBuilder builder)
    {
        builder.ConfigureServices((hostContext, services) =>
        {
            var config = hostContext.Configuration;

            services.AddInfrastructure(config);

            services.Configure<EcbExchangeRateProviderSettings>(config.GetSection("EcbExchangeRateProvider"));

            services.AddHttpClient<IEcbExchangeRateProvider, EcbExchangeRateProvider>();

            services.ConfigureQuartz();
        });
    }


}