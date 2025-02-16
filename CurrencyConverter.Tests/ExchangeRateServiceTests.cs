using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using CurrencyConversion.Infrastructure.Repositories;
using CurrencyConversion.Infrastructure.Interface;
using CurrencyConversion.Domain.Entities;
using System.Text.Json;

namespace CurrencyConversion.Tests
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<HttpMessageHandler> httpMessageHandlerMock;
        private readonly HttpClient httpClient;
        private readonly IMemoryCache cache;
        private readonly Mock<ILogger<ExchangeRateService>> loggerMock;
        private readonly ICurrencyExchangeRateservice exchangeRateService;

        public ExchangeRateServiceTests()
        {
            this.httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(httpMessageHandlerMock.Object) { BaseAddress = new Uri("https://api.frankfurter.app/") };
            this.cache = new MemoryCache(new MemoryCacheOptions());
            this.loggerMock = new Mock<ILogger<ExchangeRateService>>();
            var excludedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" };

            exchangeRateService = new ExchangeRateService(httpClient, cache, loggerMock.Object, excludedCurrencies);
        }

        [Fact]
        public async Task GetLatestExchangeRatesAsync_ReturnsRates_WhenSuccessful()
        {
            // Arrange
            var expectedResponse = new ExchangeRateResponse { BaseCurrency = "EUR", Rates = new Dictionary<string, decimal> { { "USD", 1.1m } } };
            var jsonResponse = JsonSerializer.Serialize(expectedResponse);

            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            // Act
            var result = await exchangeRateService.GetLatestExchangeRatesAsync("EUR");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EUR", result.BaseCurrency);
            Assert.True(result.Rates.ContainsKey("USD"));
        }

        [Fact]
        public async Task ConvertCurrencyAsync_ThrowsException_WhenUsingExcludedCurrency()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => exchangeRateService.ConvertCurrencyAsync("EUR", "TRY", 100));
            Assert.Contains("Conversion involving EUR or TRY is not allowed.", ex.Message);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_ThrowsException_WhenInvalidCurrency()
        {
            // Arrange
            var expectedResponse = new ExchangeRateResponse { BaseCurrency = "EUR", Rates = new Dictionary<string, decimal> { { "USD", 1.1m } } };
            var jsonResponse = JsonSerializer.Serialize(expectedResponse);

            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(jsonResponse) });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => exchangeRateService.ConvertCurrencyAsync("EUR", "GBP", 100));
            Assert.Contains("Invalid currency", ex.Message);
        }
    }
}