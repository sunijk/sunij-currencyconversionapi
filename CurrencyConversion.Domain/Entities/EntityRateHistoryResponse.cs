namespace CurrencyConversion.Domain.Entities
{
    public class EntityRateHistoryResponse
    {
    
        public decimal Amount { get; set; }

        public string BaseCurrency { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }
}
