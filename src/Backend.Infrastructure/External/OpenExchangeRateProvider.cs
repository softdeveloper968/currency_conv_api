using Backend.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Backend.Infrastructure.External
{
    public class OpenExchangeRateProvider : IRateProvider
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _latestEndpoint;
        private readonly string _currenciesEndpoint;
        private readonly string _defaultBaseCurrency;
        private readonly TimeSpan _cacheDuration;

        public OpenExchangeRateProvider(HttpClient http, IMemoryCache cache, IConfiguration config)
        {
            _http = http;
            _cache = cache;
            _config = config;

            _apiKey = config["OpenExchange:ApiKey"] ?? string.Empty;
            _baseUrl = config["OpenExchange:BaseUrl"] ?? "https://openexchangerates.org/api";
            _latestEndpoint = config["OpenExchange:LatestEndpoint"] ?? "latest.json";
            _currenciesEndpoint = config["OpenExchange:CurrenciesEndpoint"] ?? "currencies.json";
            _defaultBaseCurrency = config["OpenExchange:DefaultBaseCurrency"] ?? "USD";
            _cacheDuration = TimeSpan.FromMinutes(
                int.TryParse(config["OpenExchange:CacheDurationMinutes"], out var min) ? min : 60
            );
        }

        public async Task<Dictionary<string, decimal>> GetRatesAsync(string baseCurrency, CancellationToken ct = default)
        {
            baseCurrency = baseCurrency.ToUpperInvariant();
            var cacheKey = $"rates:{baseCurrency}";

            if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cachedRates) && cachedRates != null)
                return cachedRates;

            // ✅ Only USD is allowed for free plan
            var effectiveBase = _defaultBaseCurrency;
            var url = $"{_baseUrl}/{_latestEndpoint}?app_id={_apiKey}";

            try
            {
                var response = await _http.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadFromJsonAsync<OpenExchangeLatestResponse>(cancellationToken: ct);
                if (data == null)
                    throw new InvalidOperationException("Failed to parse OpenExchangeRates response.");

                var rates = data.Rates.ToDictionary(
                    kv => kv.Key.ToUpperInvariant(),
                    kv => Convert.ToDecimal(kv.Value)
                );

                _cache.Set(cacheKey, rates, _cacheDuration);

                // Convert base if needed
                if (!baseCurrency.Equals("USD", StringComparison.OrdinalIgnoreCase) && rates.ContainsKey(baseCurrency))
                {
                    var baseRate = rates[baseCurrency];
                    var adjusted = rates.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value / baseRate
                    );
                    return adjusted;
                }

                return rates;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error fetching rates: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default)
        {
            const string cacheKey = "currencies:list";
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cached) && cached != null)
                return cached;

            var url = $"{_baseUrl}/{_currenciesEndpoint}";
            var resp = await _http.GetFromJsonAsync<Dictionary<string, string>>(url, ct);
            var dict = resp ?? new Dictionary<string, string>();
            _cache.Set(cacheKey, dict, TimeSpan.FromHours(24));

            return dict;
        }

        private class OpenExchangeLatestResponse
        {
            public long Timestamp { get; set; }
            public string Base { get; set; } = string.Empty;
            public Dictionary<string, double> Rates { get; set; } = new();
        }
    }
}
