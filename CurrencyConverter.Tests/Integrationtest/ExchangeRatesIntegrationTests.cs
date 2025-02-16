using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CurrencyConversion.API;
using CurrencyConversion.Domain.Entities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using FluentAssertions;

namespace CurrencyConversion.Tests.Integration
{
    public class ExchangeRatesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        public ExchangeRatesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.client = factory.CreateClient();
        }

        private void AuthenticateClient(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetLatestRates_AuthenticatedUser_ReturnsOk()
        {
            // Arrange
            AuthenticateClient("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjcxMzUzMjJlLTVmZjYtNGQ1ZC04YmY3LWFmNzgxOWFlOWE1ZiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTczOTgyMDAwMSwiaXNzIjoiaHR0cHM6Ly9hdXRoLmN1cnJuY3l0ZXN0LmNvbSJ9.pbpW3N2SJfi_XKKse5GQOY7CD4LAgfPqZ02avUJ85c8");

            // Act
            var response = await client.GetAsync("api/exchangerates/latestrate?baseCurrency=EUR");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExchangeRateResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.Equal("EUR", result.BaseCurrency);
        }

        [Fact]
        public async Task Convert_ValidRequest_ReturnsOk()
        {
            // Arrange
            AuthenticateClient("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjcxMzUzMjJlLTVmZjYtNGQ1ZC04YmY3LWFmNzgxOWFlOWE1ZiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTczOTgyMDAwMSwiaXNzIjoiaHR0cHM6Ly9hdXRoLmN1cnJuY3l0ZXN0LmNvbSJ9.pbpW3N2SJfi_XKKse5GQOY7CD4LAgfPqZ02avUJ85c8");

            // Act
            var response = await client.GetAsync("api/exchangerates/convert?from=EUR&to=USD&amount=100");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("convertedAmount", content);
        }

        [Fact]
        public async Task Convert_InvalidCurrency_ReturnsBadRequest()
        {
            // Arrange
            AuthenticateClient("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjcxMzUzMjJlLTVmZjYtNGQ1ZC04YmY3LWFmNzgxOWFlOWE1ZiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTczOTgyMDAwMSwiaXNzIjoiaHR0cHM6Ly9hdXRoLmN1cnJuY3l0ZXN0LmNvbSJ9.pbpW3N2SJfi_XKKse5GQOY7CD4LAgfPqZ02avUJ85c8");

            // Act
            var response = await client.GetAsync("api/exchangerates/convert?from=EUR&to=TRY&amount=100");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ValidRequest_ReturnsOk()
        {
            // Arrange
            AuthenticateClient("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjcxMzUzMjJlLTVmZjYtNGQ1ZC04YmY3LWFmNzgxOWFlOWE1ZiIsInJvbGUiOiJBZG1pbiIsImV4cCI6MTczOTgyMDAwMSwiaXNzIjoiaHR0cHM6Ly9hdXRoLmN1cnJuY3l0ZXN0LmNvbSJ9.pbpW3N2SJfi_XKKse5GQOY7CD4LAgfPqZ02avUJ85c8");

            // Act
            var response = await client.GetAsync("api/exchangerates/history?baseCurrency=EUR&start=2024-01-01&end=2024-01-31");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("rates", content);
        }

        [Fact]
        public async Task UnauthorizedUser_CannotAccessEndpoints()
        {
            // Act
            var response = await client.GetAsync("api/exchangerates/latestrate?baseCurrency=EUR");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
