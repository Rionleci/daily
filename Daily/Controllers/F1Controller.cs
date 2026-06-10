using Daily.DataLayer.Contracts;
using Daily.DataLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daily.Controllers;

[Authorize]
[Route("f1")]
public class F1Controller : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "media.formula1.com",
        "www.formula1.com"
    };

    public F1Controller(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet("image")]
    public async Task<IActionResult> Image([FromQuery] string url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return BadRequest();

        if (!AllowedHosts.Contains(uri.Host))
            return BadRequest();

        var client = _httpFactory.CreateClient("F1Media");
        var (bytes, contentType) = await FetchImageAsync(client, uri, ct);

        if (bytes is null && F1MediaHelper.BarcelonaFallbackUrl is { } fallback &&
            !string.Equals(url, fallback, StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(fallback, UriKind.Absolute, out var fallbackUri))
                (bytes, contentType) = await FetchImageAsync(client, fallbackUri, ct);
        }

        if (bytes is null) return NotFound();

        Response.Headers.CacheControl = "public, max-age=86400";
        return File(bytes, contentType ?? "image/png");
    }

    private static async Task<(byte[]? Bytes, string? ContentType)> FetchImageAsync(
        HttpClient client, Uri uri, CancellationToken ct)
    {
        using var response = await client.GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode) return (null, null);

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        return (bytes, contentType);
    }

    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] F1TranslateRequest request, [FromServices] ITranslationService translation, CancellationToken ct)
    {
        if (request.Fields is null || request.Fields.Count == 0)
            return BadRequest();

        var translated = new Dictionary<string, string>();
        foreach (var (key, text) in request.Fields)
        {
            if (string.IsNullOrWhiteSpace(text)) continue;
            translated[key] = await translation.TranslateEnToDeAsync(text, ct);
        }

        return Json(translated);
    }
}

public record F1TranslateRequest(Dictionary<string, string> Fields);
