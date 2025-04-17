using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Factories;
using CurrencyWalletSystem.Infrastructure.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWalletSystem.Tests.Infrastructure.Factories
{
    public class WalletStrategyFactoryTests
    {
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<AddFundsStrategy> _mockAddFundsStrategy;
        private readonly Mock<SubtractFundsStrategy> _mockSubtractFundsStrategy;
        private readonly Mock<ForceSubtractFundsStrategy> _mockForceSubtractFundsStrategy;
        private readonly WalletStrategyFactory _walletStrategyFactory;

        public WalletStrategyFactoryTests()
        {
            // Create mock strategy instances
            _mockAddFundsStrategy = new Mock<AddFundsStrategy>();
            _mockSubtractFundsStrategy = new Mock<SubtractFundsStrategy>();
            _mockForceSubtractFundsStrategy = new Mock<ForceSubtractFundsStrategy>();

            // Create the mock IServiceProvider
            _mockServiceProvider = new Mock<IServiceProvider>();

            // Set up the mock service provider to return the mocked strategies
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(AddFundsStrategy)))
                .Returns(_mockAddFundsStrategy.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(SubtractFundsStrategy)))
                .Returns(_mockSubtractFundsStrategy.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(ForceSubtractFundsStrategy)))
                .Returns(_mockForceSubtractFundsStrategy.Object);

            // Create the factory instance with the mock service provider
            _walletStrategyFactory = new WalletStrategyFactory(_mockServiceProvider.Object);
        }

        [Fact]
        public void GetStrategy_ShouldReturnAddFundsStrategy_WhenAddFundsStrategyTypeIsPassed()
        {
            // Act
            var strategy = _walletStrategyFactory.GetStrategy(WalletStrategyType.AddFunds);

            // Assert
            Assert.IsAssignableFrom<AddFundsStrategy>(strategy);
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(AddFundsStrategy)), Times.Once);
        }

        [Fact]
        public void GetStrategy_ShouldReturnSubtractFundsStrategy_WhenSubtractFundsStrategyTypeIsPassed()
        {
            // Act
            var strategy = _walletStrategyFactory.GetStrategy(WalletStrategyType.SubtractFunds);

            // Assert
            Assert.IsAssignableFrom<SubtractFundsStrategy>(strategy);
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(SubtractFundsStrategy)), Times.Once);
        }

        [Fact]
        public void GetStrategy_ShouldReturnForceSubtractFundsStrategy_WhenForceSubtractFundsStrategyTypeIsPassed()
        {
            // Act
            var strategy = _walletStrategyFactory.GetStrategy(WalletStrategyType.ForceSubtractFunds);

            // Assert
            Assert.IsAssignableFrom<ForceSubtractFundsStrategy>(strategy);
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(ForceSubtractFundsStrategy)), Times.Once);
        }

        [Fact]
        public void GetStrategy_ShouldThrowArgumentOutOfRangeException_WhenUnknownStrategyTypeIsPassed()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                _walletStrategyFactory.GetStrategy((WalletStrategyType)999)); // Using an invalid strategy type

            Assert.Equal("Unknown strategy: 999 (Parameter 'strategyType')", exception.Message);
        }
    }
}
