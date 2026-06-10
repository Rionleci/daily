using System.Text.Json;
using Daily.Models;
using Microsoft.Extensions.Options;

namespace Daily.DataLayer.ApiClients;

public class ApiFootballClient
{
    private readonly HttpClient _http;
    private readonly ApiSettings _settings;

    public ApiFootballClient(HttpClient http, IOptions<ApiSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
        _http.BaseAddress = new Uri("https://v3.football.api-sports.io/");
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiFootballKey);

    public async Task<IReadOnlyList<FootballLeague>> GetStandingsAsync(int leagueId, string leagueName, CancellationToken ct = default)
    {
        if (!IsConfigured) return [];

        try
        {
            var year = DateTime.UtcNow.Year;
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"standings?league={leagueId}&season={year}");
            request.Headers.Add("x-apisports-key", _settings.ApiFootballKey);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return [];

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var standings = new List<FootballStandingRow>();
            var responseArray = doc.RootElement.GetProperty("response");

            if (responseArray.GetArrayLength() > 0)
            {
                var table = responseArray[0].GetProperty("league").GetProperty("standings")[0];
                var pos = 1;
                foreach (var row in table.EnumerateArray().Take(6))
                {
                    var all = row.GetProperty("all");
                    standings.Add(new FootballStandingRow(
                        pos++,
                        row.GetProperty("team").GetProperty("name").GetString() ?? "",
                        all.GetProperty("played").GetInt32(),
                        all.GetProperty("win").GetInt32(),
                        all.GetProperty("draw").GetInt32(),
                        all.GetProperty("lose").GetInt32(),
                        all.GetProperty("goals").GetProperty("for").GetInt32(),
                        all.GetProperty("goals").GetProperty("against").GetInt32(),
                        row.GetProperty("points").GetInt32()));
                }
            }

            if (standings.Count == 0) return [];
            return [new FootballLeague(leagueName, "bi-trophy", standings, [], [])];
        }
        catch
        {
            return [];
        }
    }
}
