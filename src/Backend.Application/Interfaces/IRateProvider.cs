namespace Backend.Application.Interfaces
{
    public interface IRateProvider
    {
        Task<Dictionary<string, decimal>> GetRatesAsync(string baseCurrency, CancellationToken ct = default);
        Task<Dictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default);
    }
}