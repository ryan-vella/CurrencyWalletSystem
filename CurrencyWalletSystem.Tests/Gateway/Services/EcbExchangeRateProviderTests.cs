using CurrencyWalletSystem.Gateway.Services;
using CurrencyWalletSystem.Gateway.Settings;
using Microsoft.Extensions.Options;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyWalletSystem.Tests.Gateway.Services
{
    public class EcbExchangeRateProviderTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly EcbExchangeRateProvider _provider;

        private const string SampleXml = @"
            <gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
              <Cube>
                <Cube time='2023-12-01'>
                  <Cube currency='USD' rate='1.1000'/>
                  <Cube currency='GBP' rate='0.8600'/>
                </Cube>
              </Cube>
            </gesmes:Envelope>";

        public EcbExchangeRateProviderTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://dummy-url.com")
            };

            var options = Options.Create(new EcbExchangeRateProviderSettings
            {
                Url = "https://dummy-url.com/ecb"
            });

            _provider = new EcbExchangeRateProvider(_httpClient, options);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnParsedRates_WhenResponseIsValid()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SampleXml)
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith("/ecb")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var rates = await _provider.GetLatestRatesAsync();

            // Assert
            Assert.NotNull(rates);
            Assert.Equal(2, rates.Count);

            var usd = rates.FirstOrDefault(r => r.Currency == "USD");
            Assert.NotNull(usd);
            Assert.Equal(1.1000m, usd.Rate);
            Assert.Equal(new DateTime(2023, 12, 1), usd.Date);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldThrow_WhenHttpFails()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _provider.GetLatestRatesAsync());
        }
    }
}
