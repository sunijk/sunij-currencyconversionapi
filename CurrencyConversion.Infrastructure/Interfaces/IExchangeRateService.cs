using CurrencyConversion.Domain.Entities;

namespace CurrencyConversion.Infrastructure.Interface
{
    public interface ICurrencyExchangeRateservice
    {
        Task<ExchangeRateResponse> GetLatestExchangeRatesAsync(string baseCurrency);
        Task<decimal> ConvertCurrencyAsync(string fromDate, string toDate, decimal amount);
        Task<EntityRateHistoryResponse> GetHistoricalExchangeRatesAsync(string baseCurrency, DateTime start, DateTime end, int page, int pageSize);
    }
}
