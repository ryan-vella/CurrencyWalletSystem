using CurrencyWalletSystem.Infrastructure.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWalletSystem.Tests.Infrastructure.Strategies
{
    public class AddFundsStrategyTests
    {
        [Fact]
        public void Execute_ShouldAddAmountToCurrentBalance()
        {
            // Arrange
            var strategy = new AddFundsStrategy();
            var currentBalance = 100m;
            var amount = 50m;

            // Act
            var result = strategy.Execute(currentBalance, amount);

            // Assert
            Assert.Equal(150m, result);
        }
    }
}
