using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Models;
using CurrencyWalletSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CurrencyWalletSystem.Tests.Infrastructure.Services
{
    public class CurrencyRateCacheTests
    {
        private readonly IMemoryCache _memoryCache;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public CurrencyRateCacheTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;
        }

        [Fact]
        public void SetRates_ShouldStoreRatesInCache()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var cache = new CurrencyRateCache(_memoryCache, context);
            var rates = new List<CurrencyRate>
        {
            new CurrencyRate { Currency = "USD", Rate = 1.2M, Date = DateTime.UtcNow }
        };

            // Act
            cache.SetRates(rates);
            var result = _memoryCache.TryGetValue("CurrencyRates", out List<CurrencyRate>? cachedRates);

            // Assert
            Assert.True(result);
            Assert.NotNull(cachedRates);
            Assert.Single(cachedRates);
            Assert.Equal("USD", cachedRates[0].Currency);
        }

        [Fact]
        public void GetRate_ShouldReturnCorrectRateFromCache()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var cache = new CurrencyRateCache(_memoryCache, context);
            var rates = new List<CurrencyRate>
        {
            new CurrencyRate { Currency = "EUR", Rate = 0.9M, Date = DateTime.UtcNow }
        };
            cache.SetRates(rates);

            // Act
            var result = cache.GetRate("EUR");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EUR", result.Currency);
        }

        [Fact]
        public void GetRate_ShouldThrowIfRateNotFound()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var cache = new CurrencyRateCache(_memoryCache, context);
            cache.SetRates(new List<CurrencyRate>()); // empty list in cache

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => cache.GetRate("JPY"));
            Assert.Equal("Exchange rate for currency 'JPY' not found.", ex.Message);
        }

        [Fact]
        public void GetAllRates_ShouldReturnRatesFromCache()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            var cache = new CurrencyRateCache(_memoryCache, context);
            var rates = new List<CurrencyRate>
        {
            new CurrencyRate { Currency = "GBP", Rate = 0.8M, Date = DateTime.UtcNow }
        };
            cache.SetRates(rates);

            // Act
            var result = cache.GetAllRates();

            // Assert
            Assert.Single(result);
            Assert.Equal("GBP", result[0].Currency);
        }

        [Fact]
        public void GetAllRates_ShouldQueryDatabaseWhenCacheMisses()
        {
            // Arrange
            using var context = new AppDbContext(_dbOptions);
            context.CurrencyRates.AddRange(new[]
            {
            new CurrencyRate { Currency = "USD", Rate = 1.1M, Date = DateTime.UtcNow.AddDays(-2) },
            new CurrencyRate { Currency = "USD", Rate = 1.2M, Date = DateTime.UtcNow },
            new CurrencyRate { Currency = "EUR", Rate = 0.85M, Date = DateTime.UtcNow }
        });
            context.SaveChanges();

            var cache = new CurrencyRateCache(_memoryCache, context);

            // Act
            var result = cache.GetAllRates();

            // Assert
            Assert.Equal(2, result.Count); // latest per currency
            Assert.Contains(result, r => r.Currency == "USD" && r.Rate == 1.2M);
            Assert.Contains(result, r => r.Currency == "EUR");
        }
    }
}