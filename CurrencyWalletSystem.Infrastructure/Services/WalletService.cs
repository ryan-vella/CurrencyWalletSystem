using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Factories;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyWalletSystem.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWalletStrategyFactory _strategyFactory;
        private readonly ICurrencyRateCache _rateCache;

        public WalletService(AppDbContext dbContext, IWalletStrategyFactory strategyFactory, ICurrencyRateCache rateCache)
        {
            _dbContext = dbContext;
            _strategyFactory = strategyFactory;
            _rateCache = rateCache;
        }

        /// <inheritdoc/>
        public async Task<Wallet> CreateWalletAsync(string currency)
        {
            var wallet = new Wallet
            {
                Balance = 0,
                Currency = currency.ToUpper(),
            };

            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();
            return wallet;
        }

        /// <inheritdoc/>
        public async Task<decimal> GetWalletBalanceAsync(int walletId, string? targetCurrency = null)
        {
            var wallet = await GetWalletByIdAsync(walletId);

            if (string.IsNullOrEmpty(targetCurrency) || targetCurrency.Equals(wallet.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return wallet.Balance;
            }

            var rate = _rateCache.GetRate(targetCurrency);

            return wallet.Balance * rate.Rate;
        }

        /// <inheritdoc/>
        public async Task<string?> GetBaseCurrencyAsync(int walletId)
        {
            var wallet = await GetWalletByIdAsync(walletId);

            return wallet.Currency;
        }

        /// <inheritdoc/>
        public async Task AdjustWalletBalanceAsync(int walletId, decimal amount, string currency, WalletStrategyType strategy)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive");
            }

            var wallet = await GetWalletByIdAsync(walletId);

            var convertedAmount = ConvertAmountAsync(amount, currency.ToUpper(), wallet.Currency);

            var strategyImpl = _strategyFactory.GetStrategy(strategy);

            wallet.Balance = strategyImpl.Execute(wallet.Balance, convertedAmount);

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a wallet by its ID.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The wallet with the specified ID.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the wallet is not found.</exception>
        private async Task<Wallet> GetWalletByIdAsync(int walletId)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            return wallet ?? throw new KeyNotFoundException("Wallet not found");
        }

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="amount">The amount to convert.</param>
        /// <param name="fromCurrency">The currency code of the amount.</param>
        /// <param name="toCurrency">The target currency code.</param>
        /// <returns>The converted amount in the target currency.</returns>
        /// <exception cref="InvalidOperationException">Thrown when exchange rate data is missing.</exception>
        private decimal ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            var fromRate = _rateCache.GetRate(fromCurrency)?.Rate ?? throw new InvalidOperationException($"Exchange rate data for '{fromCurrency}' is missing.");
            var toRate = _rateCache.GetRate(toCurrency)?.Rate ?? throw new InvalidOperationException($"Exchange rate data for '{toCurrency}' is missing.");

            var conversionRate = toRate / fromRate;
            return amount * conversionRate;
        }
    }
}
