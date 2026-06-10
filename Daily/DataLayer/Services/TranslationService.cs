using System.Text.Json;
using Daily.DataLayer.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public TranslationService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<string> TranslateEnToDeAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var cacheKey = $"tr_en_de_{text.GetHashCode()}";
        if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return cached;

        var chunks = SplitText(text, 450);
        var parts = new List<string>();

        foreach (var chunk in chunks)
        {
            var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(chunk)}&langpair=en|de";
            using var doc = await JsonDocument.ParseAsync(await _http.GetStreamAsync(url, ct), cancellationToken: ct);

            if (doc.RootElement.TryGetProperty("responseStatus", out var status) &&
                status.GetInt32() != 200)
                throw new InvalidOperationException($"Translation API status {status.GetInt32()}");

            var translated = doc.RootElement
                .GetProperty("responseData")
                .GetProperty("translatedText")
                .GetString() ?? chunk;

            parts.Add(translated);
        }

        var result = string.Join("", parts);
        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
        return result;
    }

    private static IEnumerable<string> SplitText(string text, int maxLen)
    {
        if (text.Length <= maxLen) return [text];

        var parts = new List<string>();
        var remaining = text;
        while (remaining.Length > maxLen)
        {
            var splitAt = remaining.LastIndexOf(' ', maxLen);
            if (splitAt <= 0) splitAt = maxLen;
            parts.Add(remaining[..splitAt]);
            remaining = remaining[splitAt..].TrimStart();
        }
        if (remaining.Length > 0) parts.Add(remaining);
        return parts;
    }
}
