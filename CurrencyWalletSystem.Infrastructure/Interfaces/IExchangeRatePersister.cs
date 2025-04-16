using CurrencyWalletSystem.Gateway.Models;

namespace CurrencyWalletSystem.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines a contract for persisting exchange rates into a data store.
    /// </summary>
    public interface IExchangeRatePersister
    {
        /// <summary>
        /// Persists a collection of exchange rates asynchronously.
        /// </summary>
        /// <param name="rates">The collection of exchange rates to persist.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PersistAsync(IEnumerable<ExchangeRate> rates);
    }
}
