using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using CurrencyConversion.API.Controllers;
using CurrencyConversion.Infrastructure.Interface;
using CurrencyConversion.Domain.Entities;
using System.Collections.Generic;
using CurrencyConversion.Infrastructure.Repositories;
using CurrencyConversion.Infrastructure.Provider;
using CurrencyConversion.Infrastructure.Interfaces;

public class ExchangeRatesControllerTests
{
    private readonly Mock<ICurrencyExchangeRateservice> mockExchangeRateService;
    private readonly Mock<ILogger<CurrencyExchangeRatesController>> mockLogger;
    private readonly Mock<ICurrencyProviderFactory> mockProviderFactory;
    private readonly CurrencyExchangeRatesController controller;

    public ExchangeRatesControllerTests()
    {
        this.mockExchangeRateService = new Mock<ICurrencyExchangeRateservice>();
        this.mockLogger = new Mock<ILogger<CurrencyExchangeRatesController>>();
        this.mockProviderFactory = new Mock<ICurrencyProviderFactory>();
        this.controller = new CurrencyExchangeRatesController(mockProviderFactory.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetLatestRates_ShouldReturnOk_WhenValidProvider()
    {
        // Arrange
        var providerName = "frankfurter";
        var baseCurrency = "EUR";
        var mockProvider = new Mock<ICurrencyProvider>();

        var expectedRates = new ExchangeRateResponse
        {
            BaseCurrency = baseCurrency,
            Rates = new Dictionary<string, decimal> { { "USD", 1.2M }, { "GBP", 0.85M } },
            Date = DateTime.UtcNow
        };

        mockProvider.Setup(p => p.GetLatestExchangeRatesAsync(baseCurrency))
                    .ReturnsAsync(expectedRates);

        mockProviderFactory.Setup(pf => pf.GetProvider())
                            .Returns(mockProvider.Object);

        // Act
        var result = await controller.GetLatestRates(baseCurrency);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ExchangeRateResponse>(okResult.Value);
        Assert.Equal("EUR", returnValue.BaseCurrency);
    }

    [Fact]
    public async Task Convert_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        decimal convertedAmount = 110m;
        var mockProvider = new Mock<ICurrencyProvider>();

        mockProvider.Setup(p => p.ConvertCurrencyAsync("EUR", "USD", 100)).ReturnsAsync(convertedAmount);
        mockProviderFactory.Setup(pf => pf.GetProvider())
                            .Returns(mockProvider.Object);

        // Act
        var result = await controller.ConvertCurrency("EUR", "USD", 100);
        //
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<CurrencyConversionReponse>(okResult.Value);
        Assert.Equal(110, returnValue.ConvertedAmount);
    }

    [Fact]
    public async Task Convert_InvalidCurrency_ReturnsBadRequest()
    {
        // Arrange
        var mockProvider = new Mock<ICurrencyProvider>();
        mockProvider.Setup(s => s.ConvertCurrencyAsync("EUR", "TRY", 100m)).ThrowsAsync(new ArgumentException("Conversion involving TRY is not allowed."));
        mockProviderFactory.Setup(pf => pf.GetProvider())
                          .Returns(mockProvider.Object);
        // Act
        var result = await controller.ConvertCurrency("EUR", "TRY", 100m);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Conversion involving TRY is not allowed.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetHistoricalRates_ReturnsOkResult()
    {
        // Arrange
        var mockProvider = new Mock<ICurrencyProvider>();
        var historyResponse = new EntityRateHistoryResponse
        {
            Amount = 1,
            BaseCurrency = "EUR",
            Rates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2025-01-31", new Dictionary<string, decimal> { { "USD", 1.2m } } }
            }
        };
        mockProvider.Setup(s => s.GetHistoricalExchangeRatesAsync("EUR", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10)).ReturnsAsync(historyResponse);
        mockProviderFactory.Setup(pf => pf.GetProvider())
                          .Returns(mockProvider.Object);
        // Act
        var result = await controller.GetHistoricalRates("EUR", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<EntityRateHistoryResponse>(okResult.Value);
        Assert.Equal("EUR", returnValue.BaseCurrency);
    }
}
