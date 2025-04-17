using CurrencyWalletSystem.Api.Controllers;
using CurrencyWalletSystem.Infrastructure.Enums;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CurrencyWalletSystem.Tests.Api.Controllers
{
    public class WalletControllerTests
    {
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<ILogger<WalletController>> _loggerMock;
        private readonly WalletController _controller;

        public WalletControllerTests()
        {
            _walletServiceMock = new Mock<IWalletService>();
            _loggerMock = new Mock<ILogger<WalletController>>();
            _controller = new WalletController(_walletServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateWallet_ShouldReturnCreated()
        {
            // Arrange
            var currency = "USD";
            var wallet = new Wallet { Id = 1, Currency = currency, Balance = 0m };
            _walletServiceMock.Setup(s => s.CreateWalletAsync(currency)).ReturnsAsync(wallet);

            // Act
            var result = await _controller.CreateWallet(currency);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Wallet>(createdResult.Value);
            Assert.Equal(wallet.Id, returnValue.Id);
        }

        [Fact]
        public async Task GetWalletBalance_ShouldReturnBalanceWithProvidedCurrency()
        {
            // Arrange
            int walletId = 1;
            string currency = "USD";
            decimal balance = 100m;

            _walletServiceMock.Setup(s => s.GetWalletBalanceAsync(walletId, currency)).ReturnsAsync(balance);
            _walletServiceMock.Setup(s => s.GetBaseCurrencyAsync(walletId)).ReturnsAsync("USD");

            // Act
            var result = await _controller.GetWalletBalance(walletId, currency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expected = new { WalletId = 1, Balance = 100.0m, Currency = "USD" };
            Assert.Equal(expected.WalletId, okResult.Value.GetType().GetProperty("WalletId")?.GetValue(okResult.Value));
            Assert.Equal(expected.Balance, okResult.Value.GetType().GetProperty("Balance")?.GetValue(okResult.Value));
            Assert.Equal(expected.Currency, okResult.Value.GetType().GetProperty("Currency")?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task GetWalletBalance_ShouldUseBaseCurrency_WhenCurrencyIsNull()
        {
            // Arrange
            int walletId = 1;
            string baseCurrency = "EUR";
            decimal balance = 100m;

            _walletServiceMock.Setup(s => s.GetWalletBalanceAsync(walletId, null)).ReturnsAsync(balance);
            _walletServiceMock.Setup(s => s.GetBaseCurrencyAsync(walletId)).ReturnsAsync(baseCurrency);

            // Act
            var result = await _controller.GetWalletBalance(walletId, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value!;
            var expected = new { WalletId = 1, Balance = 100.0m, Currency = "EUR" };
            Assert.Equal(expected.WalletId, okResult.Value.GetType().GetProperty("WalletId")?.GetValue(okResult.Value));
            Assert.Equal(expected.Balance, okResult.Value.GetType().GetProperty("Balance")?.GetValue(okResult.Value));
            Assert.Equal(expected.Currency, okResult.Value.GetType().GetProperty("Currency")?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task AdjustBalance_ShouldReturnNoContent_WhenStrategyIsValid()
        {
            // Arrange
            int walletId = 1;
            decimal amount = 50m;
            string currency = "USD";
            string strategy = "AddFunds";

            // Act
            var result = await _controller.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _walletServiceMock.Verify(s => s.AdjustWalletBalanceAsync(walletId, amount, currency, WalletStrategyType.AddFunds), Times.Once);
        }

        [Fact]
        public async Task AdjustBalance_ShouldReturnBadRequest_WhenStrategyIsInvalid()
        {
            // Arrange
            int walletId = 1;
            decimal amount = 50m;
            string currency = "USD";
            string strategy = "Invalid";

            // Act
            var result = await _controller.AdjustBalance(walletId, amount, currency, strategy);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid strategy", badRequest.Value!.ToString());
        }
    }
}
