using System.Text.Json;
using Daily.Models;
using Microsoft.Extensions.Options;

namespace Daily.DataLayer.ApiClients;

public class NewsApiClient
{
    private readonly HttpClient _http;
    private readonly ApiSettings _settings;

    public NewsApiClient(HttpClient http, IOptions<ApiSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
        _http.BaseAddress = new Uri("https://newsapi.org/v2/");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.NewsApiKey);

    public async Task<IReadOnlyList<PoliticsNews>> GetBusinessNewsAsync(CancellationToken ct = default)
    {
        if (!IsConfigured) return [];

        try
        {
            var response = await _http.GetAsync(
                $"top-headlines?category=business&language=en&pageSize=4&apiKey={_settings.NewsApiKey}", ct);
            if (!response.IsSuccessStatusCode) return [];

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("articles", out var articles)) return [];

            return articles.EnumerateArray().Select(a => new PoliticsNews(
                a.GetProperty("title").GetString() ?? "",
                a.GetProperty("description").GetString() ?? "",
                ["Auswirkungen auf globale Märkte möglich", "Weiter beobachten"],
                "Weltwirtschaft"
            )).ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<TechNews>> GetTechNewsAsync(CancellationToken ct = default)
    {
        if (!IsConfigured) return [];

        try
        {
            var response = await _http.GetAsync(
                $"top-headlines?category=technology&language=en&pageSize=4&apiKey={_settings.NewsApiKey}", ct);
            if (!response.IsSuccessStatusCode) return [];

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("articles", out var articles)) return [];

            return articles.EnumerateArray().Select(a => new TechNews(
                a.GetProperty("title").GetString() ?? "",
                a.GetProperty("description").GetString() ?? "",
                "Relevant für Tech-Stack und Entwicklungstrends.",
                "Strategische Bedeutung für digitale Geschäftsmodelle.",
                "Tech"
            )).ToList();
        }
        catch
        {
            return [];
        }
    }
}
