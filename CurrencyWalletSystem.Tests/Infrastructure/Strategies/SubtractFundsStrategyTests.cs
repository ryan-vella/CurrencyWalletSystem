using CurrencyWalletSystem.Infrastructure.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWalletSystem.Tests.Infrastructure.Strategies
{
    public class SubtractFundsStrategyTests
    {
        [Fact]
        public void Execute_ShouldSubtractAmount_WhenSufficientFunds()
        {
            // Arrange
            var strategy = new SubtractFundsStrategy();
            var currentBalance = 100m;
            var amount = 50m;

            // Act
            var result = strategy.Execute(currentBalance, amount);

            // Assert
            Assert.Equal(50m, result);
        }

        [Fact]
        public void Execute_ShouldThrowException_WhenInsufficientFunds()
        {
            // Arrange
            var strategy = new SubtractFundsStrategy();
            var currentBalance = 30m;
            var amount = 50m;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                strategy.Execute(currentBalance, amount));

            Assert.Equal("Insufficient funds.", exception.Message);
        }
    }
}
