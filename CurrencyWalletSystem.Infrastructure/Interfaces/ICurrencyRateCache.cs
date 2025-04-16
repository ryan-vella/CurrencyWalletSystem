using CurrencyWalletSystem.Infrastructure.Models;

namespace CurrencyWalletSystem.Infrastructure.Interfaces
{
    /// <summary>
    /// Represents a cache for storing and retrieving currency exchange rates.
    /// </summary>
    public interface ICurrencyRateCache
    {
        /// <summary>
        /// Stores a list of currency exchange rates in the cache.
        /// </summary>
        /// <param name="rates">The list of exchange rates to store.</param>
        void SetRates(List<CurrencyRate> rates);

        /// <summary>
        /// Retrieves a specific exchange rate for a given currency code.
        /// </summary>
        /// <param name="currencyCode">The ISO currency code (e.g., "USD", "EUR").</param>
        /// <returns>The exchange rate for the specified currency, or null if not found.</returns>
        CurrencyRate? GetRate(string currencyCode);

        /// <summary>
        /// Retrieves all exchange rates stored in the cache.
        /// </summary>
        /// <returns>A list of all cached exchange rates.</returns>
        List<CurrencyRate> GetAllRates();
    }
}