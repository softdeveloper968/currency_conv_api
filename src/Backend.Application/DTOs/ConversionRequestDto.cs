namespace Backend.Application.Dtos
{
    public record ConversionRequestDto(string FromCurrency, string ToCurrency, decimal Amount, string? UserId = null);
}