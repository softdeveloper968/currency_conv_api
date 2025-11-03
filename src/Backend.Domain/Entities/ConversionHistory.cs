namespace Backend.Domain.Entities
{
    public class ConversionHistory
    {
        public Guid ConversionId { get; set; }
        public string FromCurrency { get; set; } = null!;
        public string ToCurrency { get; set; } = null!;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTimeOffset ConversionDate { get; set; }
        public string UserId { get; set; } = null!;
    }
}