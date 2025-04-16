using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Models;

namespace CurrencyWalletSystem.Infrastructure.Interfaces
{
    /// <summary>
    /// Defines operations related to wallet creation, balance retrieval, and balance adjustments.
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Creates a new wallet using the specified base currency.
        /// </summary>
        /// <param name="currency">The base currency code for the wallet.</param>
        /// <returns>The created <see cref="Wallet"/> instance.</returns>
        Task<Wallet> CreateWalletAsync(string currency);

        /// <summary>
        /// Retrieves the current balance of the wallet, optionally converted to a target currency.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <param name="targetCurrency">The optional target currency code to convert the balance into.</param>
        /// <returns>The wallet balance in the specified or base currency.</returns>
        Task<decimal> GetWalletBalanceAsync(int walletId, string? targetCurrency = null);

        /// <summary>
        /// Adjusts the wallet's balance using the specified strategy.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <param name="amount">The amount to adjust.</param>
        /// <param name="currency">The currency of the amount.</param>
        /// <param name="strategy">The strategy to use for the adjustment (e.g., Add, Subtract, ForceSubtract).</param>
        Task AdjustWalletBalanceAsync(int walletId, decimal amount, string currency, WalletStrategyType strategy);

        /// <summary>
        /// Retrieves the base currency of the specified wallet.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The base currency code or null if the wallet does not exist.</returns>
        Task<string?> GetBaseCurrencyAsync(int walletId);
    }
}
