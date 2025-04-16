using CurrencyWalletSystem.Gateway.Models;

namespace CurrencyWalletSystem.Gateway.Interfaces
{
    /// <summary>
    /// Provides methods to retrieve the latest exchange rates from the European Central Bank (ECB).
    /// </summary>
    public interface IEcbExchangeRateProvider
    {
        /// <summary>
        /// Retrieves the latest available exchange rates from the ECB.
        /// </summary>
        /// <returns>A read-only collection of <see cref="ExchangeRate"/> representing the latest rates.</returns>
        Task<IReadOnlyCollection<ExchangeRate>> GetLatestRatesAsync();
    }
}
