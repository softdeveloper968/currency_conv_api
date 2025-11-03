namespace Backend.Application.Dtos
{
    public record ConversionResponseDto(Guid ConversionId, string FromCurrency, string ToCurrency, decimal FromAmount, decimal ToAmount, decimal ExchangeRate, DateTimeOffset ConversionDate, string UserId);
}