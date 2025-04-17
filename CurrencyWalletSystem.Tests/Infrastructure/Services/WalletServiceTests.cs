using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Factories;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Models;
using CurrencyWalletSystem.Infrastructure.Services;
using CurrencyWalletSystem.Infrastructure.Strategies;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;

namespace CurrencyWalletSystem.Tests.Infrastructure.Services
{
    public class WalletServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public WalletServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique per test
                .Options;
        }

        [Fact]
        public async Task CreateWalletAsync_ShouldCreateWalletWithUpperCurrency()
        {
            // Arrange
            using var dbContext = new AppDbContext(_dbOptions);
            var strategyFactoryMock = new Mock<IWalletStrategyFactory>();
            var rateCacheMock = new Mock<ICurrencyRateCache>();
            var service = new WalletService(dbContext, strategyFactoryMock.Object, rateCacheMock.Object);

            // Act
            var wallet = await service.CreateWalletAsync("usd");

            // Assert
            Assert.Equal("USD", wallet.Currency);
            Assert.Equal(0, wallet.Balance);
            Assert.True(wallet.Id > 0);
        }

        [Fact]
        public async Task GetWalletBalanceAsync_ShouldReturnSameCurrencyBalance()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "USD", Balance = 100 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var service = new WalletService(dbContext, Mock.Of<IWalletStrategyFactory>(), Mock.Of<ICurrencyRateCache>());

            var balance = await service.GetWalletBalanceAsync(wallet.Id, "USD");

            Assert.Equal(100, balance);
        }

        [Fact]
        public async Task GetWalletBalanceAsync_ShouldConvertToTargetCurrency()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "USD", Balance = 100 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var rateCacheMock = new Mock<ICurrencyRateCache>();
            rateCacheMock.Setup(r => r.GetRate("EUR")).Returns(new CurrencyRate { Currency = "EUR", Rate = 2m });

            var service = new WalletService(dbContext, Mock.Of<IWalletStrategyFactory>(), rateCacheMock.Object);

            var balance = await service.GetWalletBalanceAsync(wallet.Id, "EUR");

            Assert.Equal(200, balance);
        }

        [Fact]
        public async Task GetBaseCurrencyAsync_ShouldReturnCurrency()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "JPY", Balance = 50 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var service = new WalletService(dbContext, Mock.Of<IWalletStrategyFactory>(), Mock.Of<ICurrencyRateCache>());

            var currency = await service.GetBaseCurrencyAsync(wallet.Id);

            Assert.Equal("JPY", currency);
        }

        [Fact]
        public async Task AdjustWalletBalanceAsync_ShouldApplyStrategyAndSave()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "USD", Balance = 100 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var strategyMock = new Mock<IWalletBalanceStrategy>();
            strategyMock.Setup(s => s.Execute(100, 50)).Returns(150);

            var strategyFactoryMock = new Mock<IWalletStrategyFactory>();
            strategyFactoryMock.Setup(f => f.GetStrategy(WalletStrategyType.AddFunds)).Returns(strategyMock.Object);

            var rateCacheMock = new Mock<ICurrencyRateCache>();
            rateCacheMock.Setup(r => r.GetRate("USD")).Returns(new CurrencyRate { Currency = "USD", Rate = 1m });

            var service = new WalletService(dbContext, strategyFactoryMock.Object, rateCacheMock.Object);

            await service.AdjustWalletBalanceAsync(wallet.Id, 50, "USD", WalletStrategyType.AddFunds);

            var updatedWallet = await dbContext.Wallets.FindAsync(wallet.Id);
            Assert.Equal(150, updatedWallet!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalanceAsync_ShouldThrow_WhenAmountIsZeroOrNegative()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "USD", Balance = 100 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var service = new WalletService(dbContext, Mock.Of<IWalletStrategyFactory>(), Mock.Of<ICurrencyRateCache>());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AdjustWalletBalanceAsync(wallet.Id, 0, "USD", WalletStrategyType.AddFunds));
        }

        [Fact]
        public async Task GetWalletByIdAsync_ShouldThrow_WhenWalletNotFound()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var service = new WalletService(dbContext, Mock.Of<IWalletStrategyFactory>(), Mock.Of<ICurrencyRateCache>());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetWalletBalanceAsync(999));
        }

        [Fact]
        public async Task AdjustWalletBalanceAsync_ShouldConvertCurrencyBeforeApplyingStrategy()
        {
            using var dbContext = new AppDbContext(_dbOptions);
            var wallet = new Wallet { Currency = "USD", Balance = 100 };
            dbContext.Wallets.Add(wallet);
            await dbContext.SaveChangesAsync();

            var rateCacheMock = new Mock<ICurrencyRateCache>();
            rateCacheMock.Setup(r => r.GetRate("EUR")).Returns(new CurrencyRate { Currency = "EUR", Rate = 2 });
            rateCacheMock.Setup(r => r.GetRate("USD")).Returns(new CurrencyRate { Currency = "USD", Rate = 1 });

            var strategyMock = new Mock<IWalletBalanceStrategy>();
            strategyMock.Setup(s => s.Execute(It.IsAny<decimal>(), It.Is<decimal>(v => Math.Abs(v - 25) < 0.01m))).Returns(125);

            var strategyFactoryMock = new Mock<IWalletStrategyFactory>();
            strategyFactoryMock.Setup(f => f.GetStrategy(WalletStrategyType.AddFunds)).Returns(strategyMock.Object);

            var service = new WalletService(dbContext, strategyFactoryMock.Object, rateCacheMock.Object);

            await service.AdjustWalletBalanceAsync(wallet.Id, 50, "EUR", WalletStrategyType.AddFunds);

            var updated = await dbContext.Wallets.FindAsync(wallet.Id);
            Assert.Equal(125, updated!.Balance);
        }
    }
}
