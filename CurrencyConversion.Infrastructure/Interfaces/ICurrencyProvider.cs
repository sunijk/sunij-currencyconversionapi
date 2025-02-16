using CurrencyConversion.Domain.Entities;

namespace CurrencyConversion.Infrastructure.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<ExchangeRateResponse> GetLatestExchangeRatesAsync(string baseCurrency);
        Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount);
        Task<EntityRateHistoryResponse> GetHistoricalExchangeRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize);
    }
}
