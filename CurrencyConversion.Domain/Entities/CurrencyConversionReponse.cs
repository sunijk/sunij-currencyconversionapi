namespace CurrencyConversion.Domain.Entities
{
    public class CurrencyConversionReponse
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal  OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
    }
}

