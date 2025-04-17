using CurrencyWalletSystem.Infrastructure.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWalletSystem.Tests.Infrastructure.Strategies
{
    public class ForceSubtractFundsStrategyTests
    {
        [Fact]
        public void Execute_ShouldSubtractAmountRegardlessOfBalance()
        {
            // Arrange
            var strategy = new ForceSubtractFundsStrategy();
            var currentBalance = 20m;
            var amount = 50m;

            // Act
            var result = strategy.Execute(currentBalance, amount);

            // Assert
            Assert.Equal(-30m, result);
        }
    }

}
