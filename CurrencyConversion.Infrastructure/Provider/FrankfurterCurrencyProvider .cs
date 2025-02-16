using CurrencyConversion.Domain.Entities;
using CurrencyConversion.Infrastructure.Interface;
using CurrencyConversion.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConversion.Infrastructure.Provider
{
    /// <summary>
    /// Frankfurter Currency Provider
    /// </summary>
    public class FrankfurterCurrencyProvider : ICurrencyProvider
    {
        private readonly ICurrencyExchangeRateservice currencyExchangeRateservice;
        private readonly HttpClient httpClient;
        private readonly ILogger<FrankfurterCurrencyProvider> logger;
        public FrankfurterCurrencyProvider(HttpClient httpClient, ILogger<FrankfurterCurrencyProvider> logger,
            ICurrencyExchangeRateservice currencyExchangeRateService)
        {
           this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.currencyExchangeRateservice = currencyExchangeRateService;
        }
        

        public async Task<ExchangeRateResponse> GetLatestExchangeRatesAsync(string baseCurrency)
        {
            var result = await currencyExchangeRateservice.GetLatestExchangeRatesAsync(baseCurrency);

            return result;
        }

        public async Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            var result = await currencyExchangeRateservice.ConvertCurrencyAsync(fromCurrency, toCurrency, amount);
            return result;
        }

        public async Task<EntityRateHistoryResponse> GetHistoricalExchangeRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var result = await currencyExchangeRateservice.GetHistoricalExchangeRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
            return result;
        }
    }
}
