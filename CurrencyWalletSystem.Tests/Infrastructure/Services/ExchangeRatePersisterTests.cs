using CurrencyWalletSystem.Gateway.Models;
using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace CurrencyWalletSystem.Tests.Instrastructure.Services
{
    public class ExchangeRatePersisterTests
    {
        [Fact]
        public async Task PersistAsync_ShouldLogWarning_WhenRatesAreEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRatePersister>>();
            var sqlMock = new Mock<ISqlExecutor>();
            var persister = new ExchangeRatePersister(sqlMock.Object, loggerMock.Object);
            var emptyRates = new List<ExchangeRate>();

            // Act
            await persister.PersistAsync(emptyRates);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No exchange rates to persist")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            sqlMock.Verify(s => s.ExecuteAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public async Task PersistAsync_ShouldExecuteSql_WhenRatesAreProvided()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRatePersister>>();
            var sqlMock = new Mock<ISqlExecutor>();
            var persister = new ExchangeRatePersister(sqlMock.Object, loggerMock.Object);

            var rates = new List<ExchangeRate>
        {
            new ExchangeRate { Currency = "USD", Rate = 1.1M, Date = DateTime.UtcNow }
        };

            // Act
            await persister.PersistAsync(rates);

            // Assert
            sqlMock.Verify(s => s.ExecuteAsync(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Persisting")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Exchange rates persisted successfully")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PersistAsync_ShouldLogAndThrow_WhenSqlExecutionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExchangeRatePersister>>();
            var sqlMock = new Mock<ISqlExecutor>();

            sqlMock.Setup(s => s.ExecuteAsync(It.IsAny<string>(), It.IsAny<object[]>()))
                   .ThrowsAsync(new InvalidOperationException("Boom"));

            var persister = new ExchangeRatePersister(sqlMock.Object, loggerMock.Object);

            var rates = new List<ExchangeRate>
        {
            new ExchangeRate { Currency = "EUR", Rate = 0.9M, Date = DateTime.UtcNow }
        };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => persister.PersistAsync(rates));
            Assert.Equal("Boom", ex.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Error while persisting exchange rates")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}