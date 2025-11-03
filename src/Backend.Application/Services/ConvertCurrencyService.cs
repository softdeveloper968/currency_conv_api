using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;


namespace Backend.Application.Services
{
    public class ConvertCurrencyService
    {
        private readonly IRateProvider _rateProvider;
        private readonly IConversionRepository _repository;


        public ConvertCurrencyService(IRateProvider rateProvider, IConversionRepository repository)
        {
            _rateProvider = rateProvider;
            _repository = repository;
        }


        public async Task<ConversionResponseDto> ConvertAsync(ConversionRequestDto request, CancellationToken ct = default)
        {
            var rates = await _rateProvider.GetRatesAsync(request.FromCurrency, ct);
            if (!rates.TryGetValue(request.ToCurrency, out var rate))
                throw new InvalidOperationException($"Rate not found for {request.ToCurrency}");


            decimal toAmount = decimal.Multiply(request.Amount, rate);


            var entity = new ConversionHistory
            {
                ConversionId = Guid.NewGuid(),
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                FromAmount = request.Amount,
                ToAmount = toAmount,
                ExchangeRate = rate,
                ConversionDate = DateTimeOffset.UtcNow,
                UserId = request.UserId ?? "hardcoded-user"
            };


            await _repository.AddAsync(entity, ct);


            return new ConversionResponseDto(entity.ConversionId, entity.FromCurrency, entity.ToCurrency, entity.FromAmount, entity.ToAmount, entity.ExchangeRate, entity.ConversionDate, entity.UserId);
        }
    }
}