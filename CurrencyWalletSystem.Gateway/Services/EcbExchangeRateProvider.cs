using CurrencyWalletSystem.Gateway.Interfaces;
using CurrencyWalletSystem.Gateway.Models;
using CurrencyWalletSystem.Gateway.Settings;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Xml.Linq;

namespace CurrencyWalletSystem.Gateway.Services
{
    /// <summary>
    /// Provides exchange rates fetched from the European Central Bank.
    /// </summary>
    public class EcbExchangeRateProvider : IEcbExchangeRateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _ecbUrl;

        /// Initializes a new instance of the <see cref="EcbExchangeRateProvider"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making requests.</param>
        /// <param name="options">The options containing ECB URL configuration.</param>
        public EcbExchangeRateProvider(HttpClient httpClient, IOptions<EcbExchangeRateProviderSettings> options)
        {
            _httpClient = httpClient;
            _ecbUrl = options.Value.Url;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<ExchangeRate>> GetLatestRatesAsync()
        {
            var response = await _httpClient.GetAsync(_ecbUrl);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var document = XDocument.Load(stream);

            var rates = document.Descendants()
                .Where(e => e.Name.LocalName == "Cube" && e.Attribute("currency") != null)
                .Select(e => new ExchangeRate
                {
                    Currency = e.Attribute("currency")!.Value,
                    Rate = decimal.Parse(e.Attribute("rate")!.Value, CultureInfo.InvariantCulture),
                    Date = document.Descendants()
                                   .FirstOrDefault(x => x.Name.LocalName == "Cube" && x.Attribute("time") != null)?
                                   .Attribute("time")?.Value is string dateStr
                            ? DateTime.Parse(dateStr)
                            : DateTime.UtcNow
                })
                .ToList();

            return rates;
        }
    }
}
