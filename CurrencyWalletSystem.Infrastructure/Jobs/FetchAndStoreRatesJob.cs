using CurrencyWalletSystem.Gateway.Interfaces;
using CurrencyWalletSystem.Gateway.Models;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using Quartz;

namespace CurrencyWalletSystem.Infrastructure.Jobs
{
    public class FetchAndStoreRatesJob : IJob
    {
        private readonly IEcbExchangeRateProvider _provider;
        private readonly IExchangeRatePersister _persister;

        /// <summary>
        /// Initializes a new instance of the <see cref="FetchAndStoreRatesJob"/> class.
        /// </summary>
        /// <param name="provider">Service to fetch the latest exchange rates.</param>
        /// <param name="persister">Service to persist the fetched exchange rates.</param>
        public FetchAndStoreRatesJob(IEcbExchangeRateProvider provider, IExchangeRatePersister persister)
        {
            _provider = provider;
            _persister = persister;
        }

        /// <summary>
        /// Executes the job to fetch the latest exchange rates and store them in the database.
        /// This method is invoked by the Quartz scheduler.
        /// </summary>
        /// <param name="context">The context for the job execution.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var rates = await FetchRatesAsync();
            if (rates.Count > 0)
            {
                await PersistRatesAsync(rates);
            }
        }

        /// <summary>
        /// Fetches the latest exchange rates using the provider.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation to get exchange rates.</returns>
        private async Task<IReadOnlyCollection<ExchangeRate>> FetchRatesAsync()
        {
            return await _provider.GetLatestRatesAsync();
        }

        /// <summary>
        /// Persists the fetched exchange rates using the persister.
        /// </summary>
        /// <param name="rates">A dictionary of exchange rates to persist.</param>
        /// <returns>A Task representing the asynchronous operation to persist the rates.</returns>
        private async Task PersistRatesAsync(IReadOnlyCollection<ExchangeRate> rates)
        {
            await _persister.PersistAsync(rates);
        }
    }
}
