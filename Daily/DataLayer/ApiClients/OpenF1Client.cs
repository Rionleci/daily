using System.Text.Json;

namespace Daily.DataLayer.ApiClients;

public class OpenF1Client
{
    private readonly HttpClient _http;

    public OpenF1Client(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.openf1.org/v1/");
    }

    public async Task<IReadOnlyList<OpenF1Driver>> GetLatestDriversAsync(CancellationToken ct = default)
    {
        var doc = await GetJsonAsync("drivers?session_key=latest", ct);
        return doc.RootElement.EnumerateArray().Select(ParseDriver).ToList();
    }

    public async Task<IReadOnlyList<OpenF1Meeting>> GetMeetingsForYearAsync(int year, CancellationToken ct = default)
    {
        var doc = await GetJsonAsync($"meetings?year={year}", ct);
        return doc.RootElement.EnumerateArray()
            .Select(ParseMeeting)
            .Where(m => !m.IsCancelled)
            .OrderBy(m => m.DateStart)
            .ToList();
    }

    public async Task<OpenF1Meeting?> GetNextMeetingAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var doc = await GetJsonAsync($"meetings?year={year}", ct);
        var now = DateTime.UtcNow;

        var next = doc.RootElement.EnumerateArray()
            .Select(ParseMeeting)
            .Where(m => m.DateStart > now && !m.IsCancelled)
            .OrderBy(m => m.DateStart)
            .FirstOrDefault();

        return next;
    }

    public async Task<OpenF1Meeting?> FindMeetingByCircuitAsync(string circuitId, string circuitName, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var doc = await GetJsonAsync($"meetings?year={year}", ct);

        var normalizedId = circuitId.Replace("_", "").ToLowerInvariant();
        var normalizedName = circuitName.ToLowerInvariant();

        return doc.RootElement.EnumerateArray()
            .Select(ParseMeeting)
            .FirstOrDefault(m =>
                m.CircuitShortName.Replace(" ", "").ToLowerInvariant().Contains(normalizedId) ||
                normalizedName.Contains(m.CircuitShortName.ToLowerInvariant()) ||
                normalizedName.Contains(m.Location.ToLowerInvariant()));
    }

    private static OpenF1Driver ParseDriver(JsonElement d) => new(
        d.GetProperty("driver_number").GetInt32(),
        d.TryGetProperty("name_acronym", out var ac) ? ac.GetString() ?? "" : "",
        d.TryGetProperty("full_name", out var fn) ? fn.GetString() ?? "" : "",
        d.TryGetProperty("last_name", out var ln) ? ln.GetString() ?? "" : "",
        d.TryGetProperty("team_name", out var tn) ? tn.GetString() ?? "" : "",
        d.TryGetProperty("team_colour", out var tc) ? tc.GetString() ?? "333333" : "333333",
        FixHeadshotUrl(d.TryGetProperty("headshot_url", out var hs) ? hs.GetString() : null));

    private static OpenF1Meeting ParseMeeting(JsonElement m) => new(
        m.GetProperty("meeting_key").GetInt32(),
        m.TryGetProperty("meeting_name", out var mn) ? mn.GetString() ?? "" : "",
        m.TryGetProperty("location", out var loc) ? loc.GetString() ?? "" : "",
        m.TryGetProperty("country_name", out var cn) ? cn.GetString() ?? "" : "",
        m.TryGetProperty("country_code", out var cc) ? cc.GetString() ?? "" : "",
        m.TryGetProperty("country_flag", out var cf) ? cf.GetString() ?? "" : "",
        m.TryGetProperty("circuit_short_name", out var cs) ? cs.GetString() ?? "" : "",
        m.TryGetProperty("circuit_image", out var ci) ? ci.GetString() ?? "" : "",
        m.TryGetProperty("circuit_info_url", out var cu) ? cu.GetString() ?? "" : "",
        DateTime.Parse(m.GetProperty("date_start").GetString()!),
        m.TryGetProperty("is_cancelled", out var ic) && ic.GetBoolean());

    private static string FixHeadshotUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "";
        const string broken = "/d_driver_fallback_image.png/content/";
        if (url.Contains(broken))
            return "https://media.formula1.com/content/" + url.Split(broken)[1];
        return url;
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken ct)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }
}

public record OpenF1Driver(
    int DriverNumber,
    string Acronym,
    string FullName,
    string LastName,
    string TeamName,
    string TeamColour,
    string HeadshotUrl);

public record OpenF1Meeting(
    int MeetingKey,
    string MeetingName,
    string Location,
    string CountryName,
    string CountryCode,
    string CountryFlagUrl,
    string CircuitShortName,
    string CircuitImageUrl,
    string CircuitInfoUrl,
    DateTime DateStart,
    bool IsCancelled);
