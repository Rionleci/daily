using System.Globalization;
using System.Text.Json;

namespace Daily.DataLayer.ApiClients;

/// <summary>
/// Streckenlänge aus OpenF1 circuit_info_url (Multiviewer API).
/// </summary>
public class MultiviewerCircuitClient
{
    private readonly HttpClient _http;

    public MultiviewerCircuitClient(HttpClient http) => _http = http;

    public async Task<decimal?> GetTrackLengthKmAsync(string circuitInfoUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(circuitInfoUrl)) return null;

        try
        {
            var response = await _http.GetAsync(circuitInfoUrl, ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("corners", out var corners)) return null;

            var maxLength = corners.EnumerateArray()
                .Select(c => c.TryGetProperty("length", out var l) ? l.GetDouble() : 0)
                .DefaultIfEmpty(0)
                .Max();

            return maxLength > 0 ? Math.Round((decimal)(maxLength / 1000.0), 3) : null;
        }
        catch
        {
            return null;
        }
    }
}
