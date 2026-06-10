using System.Text.Json;
using Daily.Models;
using Microsoft.Extensions.Options;

namespace Daily.DataLayer.ApiClients;

public class FinnhubClient
{
    private readonly HttpClient _http;
    private readonly ApiSettings _settings;

    public FinnhubClient(HttpClient http, IOptions<ApiSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
        _http.BaseAddress = new Uri("https://finnhub.io/api/v1/");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.FinnhubApiKey);

    public async Task<decimal?> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        if (!IsConfigured) return null;

        try
        {
            var response = await _http.GetAsync(
                $"quote?symbol={Uri.EscapeDataString(symbol)}&token={_settings.FinnhubApiKey}", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;
            if (!root.TryGetProperty("c", out var current) || current.GetDecimal() == 0) return null;
            return current.GetDecimal();
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<MarketNews>> GetMarketNewsAsync(CancellationToken ct = default)
    {
        if (!IsConfigured) return [];

        try
        {
            var response = await _http.GetAsync(
                $"news?category=general&token={_settings.FinnhubApiKey}", ct);
            if (!response.IsSuccessStatusCode) return [];

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return doc.RootElement.EnumerateArray().Take(5).Select(item => new MarketNews(
                item.GetProperty("headline").GetString() ?? "",
                item.GetProperty("summary").GetString() ?? "",
                "Relevante Marktbewegung – Details in den Quellen."
            )).ToList();
        }
        catch
        {
            return [];
        }
    }
}
