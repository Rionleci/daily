using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Daily.DataLayer.ApiClients;

/// <summary>
/// Dynamische Streckenlayouts und Metadaten über die Wikipedia API.
/// Wikipedia verlangt einen aussagekräftigen User-Agent (siehe Program.cs).
/// </summary>
public class WikipediaCircuitClient
{
    private readonly HttpClient _http;
    private const string ApiBase = "https://en.wikipedia.org/w/api.php";

    public WikipediaCircuitClient(HttpClient http) => _http = http;

    public async Task<WikipediaCircuitInfo> GetCircuitInfoAsync(
        string circuitName,
        string? wikipediaUrl = null,
        CancellationToken ct = default)
    {
        try
        {
            var title = ExtractWikiTitle(wikipediaUrl) ?? circuitName;
            var layoutUrl = await GetCircuitLayoutSvgUrlAsync(title, ct);
            var (lengthKm, laps) = await GetTrackStatsAsync(title, ct);
            return new WikipediaCircuitInfo(layoutUrl, lengthKm, laps);
        }
        catch
        {
            return new WikipediaCircuitInfo("", 0, 0);
        }
    }

    private async Task<string> GetCircuitLayoutSvgUrlAsync(string pageTitle, CancellationToken ct)
    {
        using var pageDoc = await QueryAsync(new Dictionary<string, string>
        {
            ["action"] = "query",
            ["titles"] = pageTitle,
            ["prop"] = "pageimages|images",
            ["piprop"] = "name",
            ["imlimit"] = "20",
            ["format"] = "json"
        }, ct);

        if (pageDoc is null) return "";

        var page = pageDoc.RootElement.GetProperty("query").GetProperty("pages").EnumerateObject().First().Value;

        if (page.TryGetProperty("pageimage", out var pageImage))
        {
            var fileName = pageImage.GetString();
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var svgUrl = await GetFileUrlAsync(fileName, ct);
                if (!string.IsNullOrEmpty(svgUrl)) return svgUrl;
            }
        }

        if (page.TryGetProperty("images", out var images))
        {
            foreach (var img in images.EnumerateArray())
            {
                var fileTitle = img.GetProperty("title").GetString() ?? "";
                if (!fileTitle.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) continue;
                if (!IsCircuitLayoutFile(fileTitle)) continue;

                var fileName = fileTitle["File:".Length..];
                var svgUrl = await GetFileUrlAsync(fileName, ct);
                if (!string.IsNullOrEmpty(svgUrl)) return svgUrl;
            }
        }

        using var thumbDoc = await QueryAsync(new Dictionary<string, string>
        {
            ["action"] = "query",
            ["titles"] = pageTitle,
            ["prop"] = "pageimages",
            ["pithumbsize"] = "1200",
            ["format"] = "json"
        }, ct);

        if (thumbDoc is null) return "";

        var thumbPage = thumbDoc.RootElement.GetProperty("query").GetProperty("pages").EnumerateObject().First().Value;
        if (thumbPage.TryGetProperty("thumbnail", out var thumb))
            return thumb.GetProperty("source").GetString() ?? "";

        return "";
    }

    private async Task<(decimal LengthKm, int Laps)> GetTrackStatsAsync(string pageTitle, CancellationToken ct)
    {
        using var doc = await QueryAsync(new Dictionary<string, string>
        {
            ["action"] = "query",
            ["titles"] = pageTitle,
            ["prop"] = "extracts",
            ["explaintext"] = "true",
            ["format"] = "json"
        }, ct);

        if (doc is null) return (0, 0);

        var page = doc.RootElement.GetProperty("query").GetProperty("pages").EnumerateObject().First().Value;
        var extract = page.TryGetProperty("extract", out var ex) ? ex.GetString() ?? "" : "";

        decimal lengthKm = 0;
        var lengthMatch = Regex.Match(extract, @"(\d+\.?\d*)\s*km\s*\((\d+\.?\d*)\s*mi\)", RegexOptions.IgnoreCase);
        if (lengthMatch.Success)
            decimal.TryParse(lengthMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out lengthKm);

        if (lengthKm == 0)
        {
            var simpleMatch = Regex.Match(extract, @"length[:\s]+(\d+\.?\d*)\s*km", RegexOptions.IgnoreCase);
            if (simpleMatch.Success)
                decimal.TryParse(simpleMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out lengthKm);
        }

        var laps = 0;
        var lapMatch = Regex.Match(extract, @"(\d+)\s+laps", RegexOptions.IgnoreCase);
        if (lapMatch.Success)
            int.TryParse(lapMatch.Groups[1].Value, out laps);

        return (lengthKm, laps);
    }

    private async Task<string> GetFileUrlAsync(string fileName, CancellationToken ct)
    {
        using var doc = await QueryAsync(new Dictionary<string, string>
        {
            ["action"] = "query",
            ["titles"] = $"File:{fileName}",
            ["prop"] = "imageinfo",
            ["iiprop"] = "url",
            ["format"] = "json"
        }, ct);

        if (doc is null) return "";

        var page = doc.RootElement.GetProperty("query").GetProperty("pages").EnumerateObject().First().Value;
        if (!page.TryGetProperty("imageinfo", out var info) || info.GetArrayLength() == 0)
            return "";

        return info[0].GetProperty("url").GetString() ?? "";
    }

    private async Task<JsonDocument?> QueryAsync(Dictionary<string, string> parameters, CancellationToken ct)
    {
        try
        {
            var query = string.Join("&", parameters.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var response = await _http.GetAsync($"{ApiBase}?{query}", ct);
            if (!response.IsSuccessStatusCode) return null;
            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsCircuitLayoutFile(string fileTitle)
    {
        var lower = fileTitle.ToLowerInvariant();
        return lower.Contains("circuit") || lower.Contains("layout") || lower.Contains("track")
               || lower.Contains("motogp") || lower.Contains("grand prix");
    }

    private static string? ExtractWikiTitle(string? wikipediaUrl)
    {
        if (string.IsNullOrWhiteSpace(wikipediaUrl)) return null;
        var idx = wikipediaUrl.LastIndexOf("/wiki/", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        return Uri.UnescapeDataString(wikipediaUrl[(idx + 6)..]);
    }
}

public record WikipediaCircuitInfo(string LayoutSvgUrl, decimal LengthKm, int Laps);
