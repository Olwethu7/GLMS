using System.Text.Json;

namespace GLMS.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private const string API_URL = "https://api.exchangerate-api.com/v4/latest/USD";

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetUSDtoZARRate()
        {
            try
            {
                var response = await _httpClient.GetAsync(API_URL);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var exchangeData = JsonSerializer.Deserialize<ExchangeRateResponse>(jsonString);

                if (exchangeData?.Rates != null && exchangeData.Rates.TryGetValue("ZAR", out var rate))
                {
                    _logger.LogInformation($"Successfully fetched USD to ZAR rate: {rate}");
                    return rate;
                }

                _logger.LogWarning("Could not fetch rate from API, using fallback rate");
                return 19.50m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate from API");
                return 19.50m;
            }
        }

        public async Task<decimal> ConvertUSDtoZAR(decimal usdAmount)
        {
            var rate = await GetUSDtoZARRate();
            return Math.Round(usdAmount * rate, 2);
        }

        private class ExchangeRateResponse
        {
            public string? Base { get; set; }
            public Dictionary<string, decimal>? Rates { get; set; }
            public string? Date { get; set; }
        }
    }
}