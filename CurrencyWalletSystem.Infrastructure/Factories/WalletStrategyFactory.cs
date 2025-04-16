using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyWalletSystem.Infrastructure.Factories
{
    /// <summary>
    /// Factory for resolving wallet balance strategies using dependency injection.
    /// </summary>
    public class WalletStrategyFactory : IWalletStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletStrategyFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        public WalletStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IWalletBalanceStrategy GetStrategy(WalletStrategyType strategyType)
        {
            return strategyType switch
            {
                WalletStrategyType.AddFunds => ResolveStrategy<AddFundsStrategy>(),
                WalletStrategyType.SubtractFunds => ResolveStrategy<SubtractFundsStrategy>(),
                WalletStrategyType.ForceSubtractFunds => ResolveStrategy<ForceSubtractFundsStrategy>(),
                _ => throw new ArgumentOutOfRangeException(nameof(strategyType), $"Unknown strategy: {strategyType}")
            };
        }

        /// <summary>
        /// Resolves a strategy implementation from the service provider.
        /// </summary>
        /// <typeparam name="TStrategy">The type of the strategy to resolve.</typeparam>
        /// <returns>The resolved strategy implementation.</returns>
        private TStrategy ResolveStrategy<TStrategy>() where TStrategy : IWalletBalanceStrategy
        {
            return _serviceProvider.GetRequiredService<TStrategy>();
        }
    }
}
