using CurrencyWalletSystem.Gateway.Interfaces;
using CurrencyWalletSystem.Gateway.Models;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Jobs;
using Moq;
using Quartz;

namespace CurrencyWalletSystem.Tests.Infrastructure.Jobs
{
    public class FetchAndStoreRatesJobTests
    {
        private readonly Mock<IEcbExchangeRateProvider> _mockProvider;
        private readonly Mock<IExchangeRatePersister> _mockPersister;
        private readonly FetchAndStoreRatesJob _fetchAndStoreRatesJob;

        public FetchAndStoreRatesJobTests()
        {
            _mockProvider = new Mock<IEcbExchangeRateProvider>();
            _mockPersister = new Mock<IExchangeRatePersister>();
            _fetchAndStoreRatesJob = new FetchAndStoreRatesJob(_mockProvider.Object, _mockPersister.Object);
        }

        [Fact]
        public async Task Execute_ShouldFetchRatesAndPersist_WhenCalled()
        {
            // Arrange
            var rates = new List<ExchangeRate>
            {
                new ExchangeRate { Currency = "USD", Rate = 1.2m, Date = DateTime.UtcNow },
                new ExchangeRate { Currency = "EUR", Rate = 1.1m, Date = DateTime.UtcNow }
            };

            _mockProvider.Setup(provider => provider.GetLatestRatesAsync())
                         .ReturnsAsync(rates);

            // Act
            await _fetchAndStoreRatesJob.Execute(It.IsAny<IJobExecutionContext>());

            // Assert
            _mockProvider.Verify(provider => provider.GetLatestRatesAsync(), Times.Once);
            _mockPersister.Verify(persister => persister.PersistAsync(It.Is<IReadOnlyCollection<ExchangeRate>>(r => r.Count == rates.Count)), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldNotPersist_WhenNoRatesAreFetched()
        {
            // Arrange
            _mockProvider.Setup(provider => provider.GetLatestRatesAsync())
                         .ReturnsAsync(new List<ExchangeRate>());

            // Act
            await _fetchAndStoreRatesJob.Execute(It.IsAny<IJobExecutionContext>());

            // Assert
            _mockProvider.Verify(provider => provider.GetLatestRatesAsync(), Times.Once);
            _mockPersister.Verify(persister => persister.PersistAsync(It.IsAny<IReadOnlyCollection<ExchangeRate>>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldLogError_WhenFetchFails()
        {
            // Arrange
            _mockProvider.Setup(provider => provider.GetLatestRatesAsync())
                         .ThrowsAsync(new Exception("Error fetching rates"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _fetchAndStoreRatesJob.Execute(It.IsAny<IJobExecutionContext>()));

            // Verify that the exception is logged or rethrown correctly
            _mockProvider.Verify(provider => provider.GetLatestRatesAsync(), Times.Once);
        }
    }
}
