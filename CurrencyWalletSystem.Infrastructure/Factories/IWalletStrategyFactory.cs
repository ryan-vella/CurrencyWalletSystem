using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Strategies;

namespace CurrencyWalletSystem.Infrastructure.Factories
{
    /// <summary>
    /// Factory interface for retrieving wallet balance strategies.
    /// </summary>
    public interface IWalletStrategyFactory
    {
        /// <summary>
        /// Retrieves the appropriate wallet balance strategy based on the provided type.
        /// </summary>
        /// <param name="strategyType">The type of strategy to retrieve.</param>
        /// <returns>An implementation of <see cref="IWalletBalanceStrategy"/>.</returns>
        IWalletBalanceStrategy GetStrategy(WalletStrategyType strategyType);
    }
}
