using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CurrencyConversion.Infrastructure.Repositories;
using CurrencyConversion.Infrastructure.Interface;
using System.Buffers.Text;
using System.ComponentModel;
using StackExchange.Redis;
using CurrencyConversion.Infrastructure.Provider;
using System.Data.Common;
using CurrencyConversion.Infrastructure.Interfaces;
using CurrencyConversion.Domain.Entities;

namespace CurrencyConversion.API.Controllers;

/// <summary>
/// Currency Converter API
/// End point to retrieve latest exchange rates 
/// Fetch the latest exchange rates for a specific base currency(e.g., EUR). 
/// End point for currency conversion
//  End point to get historical exchange rates with pagination

/// </summary>

//[Authorize]
[ApiVersion("1.0")]
[ApiController]
[Route("api/exchangerates")]
//[Route("api/v{version:apiVersion}/exchangerates")]
public class CurrencyExchangeRatesController : ControllerBase
{
    private readonly ILogger<CurrencyExchangeRatesController> logger;

    private readonly ICurrencyProviderFactory providerFactory;

    public CurrencyExchangeRatesController(ICurrencyProviderFactory providerFactory, ILogger<CurrencyExchangeRatesController> logger)
    {
        this.providerFactory = providerFactory;
        this.logger = logger;
    }


    [HttpGet("latestrate")]
    [Authorize(Roles = "Guest,Admin")]
    [Description("Fetch the latest exchange rates for a specific base currency.")]
    public async Task<IActionResult> GetLatestRates([FromQuery] string baseCurrency = "EUR")
    {
        try
        {
            var providerInstance = this.providerFactory.GetProvider();
            var rates = await providerInstance.GetLatestExchangeRatesAsync(baseCurrency);
            return Ok(rates);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpGet("convert")]
    [Authorize(Roles = "Guest,Admin")]
    [Description("Convert amounts between different currencies.")]
    public async Task<IActionResult> ConvertCurrency([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount)
    {
        try
        {
            var providerInstance = this.providerFactory.GetProvider();
            var convertedAmount = await providerInstance.ConvertCurrencyAsync(from, to, amount);
            var currencyConversionReponse = new CurrencyConversionReponse
            {
                FromCurrency = from,
                ToCurrency = to,
                OriginalAmount = amount,
                ConvertedAmount = convertedAmount
            };
            return Ok(currencyConversionReponse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("history")]
    [Authorize(Roles = "Guest,Admin")]
    [Description("Retrieve historical exchange rates for a given period with pagination.")]
    public async Task<IActionResult> GetHistoricalRates([FromQuery] string baseCurrency, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var providerInstance = this.providerFactory.GetProvider();
            var history = await providerInstance.GetHistoricalExchangeRatesAsync(baseCurrency, start, end, page, pageSize);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
