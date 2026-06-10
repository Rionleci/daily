using System.Text.Json;
using Daily.Models;

namespace Daily.DataLayer.ApiClients;

public class CoinGeckoClient
{
    private readonly HttpClient _http;

    public CoinGeckoClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    }

    public async Task<MarketQuote?> GetCryptoQuoteAsync(string coinId, string displayName, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(
                $"simple/price?ids={coinId}&vs_currencies=usd&include_24hr_change=true&include_7d_change=true", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty(coinId, out var coin)) return null;

            var price = coin.GetProperty("usd").GetDecimal();
            var change24 = coin.TryGetProperty("usd_24h_change", out var c24) ? c24.GetDecimal() : 0m;
            var change7d = coin.TryGetProperty("usd_7d_change", out var c7) ? c7.GetDecimal() : 0m;

            return new MarketQuote(coinId.ToUpperInvariant(), displayName, price, change24, change7d,
                GenerateSparkline(change24 >= 0));
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<decimal> GenerateSparkline(bool up) =>
        up ? [100m, 101.2m, 100.8m, 102.1m, 103.2m, 104.1m] : [100m, 99.1m, 98.5m, 99.2m, 97.8m, 97.1m];
}
