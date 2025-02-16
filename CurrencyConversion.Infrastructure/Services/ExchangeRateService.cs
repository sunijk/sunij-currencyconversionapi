using CurrencyConversion.Domain.Entities;
using CurrencyConversion.Infrastructure.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CurrencyConversion.Infrastructure.Repositories
{
    public class ExchangeRateService : ICurrencyExchangeRateservice
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<ExchangeRateService> logger;
        // private static readonly string[] ExcludedCurrencies = { "TRY", "PLN", "THB", "MXN" };
        private readonly List<string> excludedCurrencies;
        public ExchangeRateService(HttpClient httpClient, IMemoryCache cache, ILogger<ExchangeRateService> logger, List<string> excludedCurrencies)
        {
            this.httpClient = httpClient;
            this.memoryCache = cache;
            this.logger = logger;
            this.excludedCurrencies = excludedCurrencies;
        }

        /// <summary>
        /// Get Latest Exchange Rates 
        /// Fetch the latest exchange rates for a specific base currency (e.g., EUR)
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ExchangeRateResponse> GetLatestExchangeRatesAsync(string baseCurrency)
        {
            // Fecth the latest thcurrency details from cache , if available in Cache
            if (this.memoryCache.TryGetValue($"latestthis.{baseCurrency}", out ExchangeRateResponse cachedRates) && cachedRates != null)
            {
                cachedRates.BaseCurrency = baseCurrency;
                return cachedRates;
            }

            // Invoke the API to get the Get Latest Exchange Rates
            var response = await this.httpClient.GetAsync($"https://api.frankfurter.app/latest?base={baseCurrency}");
            response.EnsureSuccessStatusCode();

            // Get the response & deserialize the response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ExchangeRateResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.Rates == null || !result.Rates.Any())
            {
                throw new InvalidOperationException("Not valid exchange rate data.");
            }

            // Save the fetched currency in  memory caching
            this.memoryCache.Set($"latestthis.{baseCurrency}", result, TimeSpan.FromMinutes(1));
            return result;
        }

        /// <summary>
        /// Convert Currency 
        /// Convert amounts between different currencies.
        /// </summary>
        /// <param name="fromCurrency"></param>
        /// <param name="toCurrency"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            // Verify the requested currency is not Unsupported/excluded currency: TRY, PLN, THB, MXN");
            if (this.excludedCurrencies.Contains(fromCurrency) || this.excludedCurrencies.Contains(toCurrency))
            {
                throw new ArgumentException($"Conversion involving {fromCurrency} or {toCurrency} is not allowed.");
            }

            // Get Latest Exchange Rates
            var rates = await GetLatestExchangeRatesAsync(fromCurrency);

            // Validate the currency
            if (!rates.Rates.ContainsKey(toCurrency))
                throw new ArgumentException($"Invalid currency: {toCurrency}");

            // Convert amounts between different currencies.

            var convertedAmount = amount * rates.Rates[toCurrency];
            return convertedAmount;
        }

        /// <summary>
        /// Get Historical ExchangeRates
        /// Retrieve historical exchange rates for a given period with pagination (e.g., 2020-01-01 to 2020-01-31, base EUR).
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<EntityRateHistoryResponse> GetHistoricalExchangeRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            // Get historical exchange rates for a given period( between startDate & endDate)
            var historicalExchangeRates = await this.httpClient.GetAsync($"https://api.frankfurter.app/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}");
            historicalExchangeRates.EnsureSuccessStatusCode();

            // Get response & Deserialize
            var jsonResponse = await historicalExchangeRates.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EntityRateHistoryResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.Rates == null || !result.Rates.Any())
            {
                throw new InvalidOperationException("Not valid exchange rate data.");
            }

            // Apply pagination logic using Skip & Take
            var pagedRates = result.Rates.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var response = new EntityRateHistoryResponse
            {
                BaseCurrency = result.BaseCurrency,
                Amount = result.Amount,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                Rates = pagedRates
            };

            return result;
        }
    }
}
