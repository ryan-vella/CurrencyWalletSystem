using CurrencyWalletSystem.Infrastructure.Data;
using CurrencyWalletSystem.Infrastructure.Interfaces;
using CurrencyWalletSystem.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyWalletSystem.Infrastructure.Services
{
    /// <summary>
    /// Provides caching functionality for currency exchange rates using in-memory caching.
    /// </summary>
    public class CurrencyRateCache : ICurrencyRateCache
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _dbContext;
        private const string CacheKey = "CurrencyRates";

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyRateCache"/> class.
        /// </summary>
        /// <param name="cache">The memory cache implementation.</param>
        public CurrencyRateCache(IMemoryCache cache, AppDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public void SetRates(List<CurrencyRate> rates)
        {
            _cache.Set(CacheKey, rates, TimeSpan.FromHours(1));
        }

        /// <inheritdoc />
        public CurrencyRate? GetRate(string currencyCode)
        {
            var rates = GetAllRates();
            var rate = rates.FirstOrDefault(r => r.Currency == currencyCode);

            if (rate is null)
            {
                throw new InvalidOperationException($"Exchange rate for currency '{currencyCode}' not found.");
            }

            return rate;
        }

        /// <inheritdoc />
        public List<CurrencyRate> GetAllRates()
        {
            if (_cache.TryGetValue(CacheKey, out List<CurrencyRate>? rates) && rates is not null && rates.Any())
            {
                return rates;
            }

            rates = _dbContext.CurrencyRates
               .GroupBy(r => r.Currency)
               .Select(g => g.OrderByDescending(r => r.Date).First())
               .ToList();

            if (rates.Any())
            {
                SetRates(rates);
            }

            return rates;
        }
    }
}
