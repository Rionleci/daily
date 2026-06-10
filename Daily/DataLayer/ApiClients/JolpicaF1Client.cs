using System.Globalization;
using System.Text.Json;

namespace Daily.DataLayer.ApiClients;

public class JolpicaF1Client
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public JolpicaF1Client(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.jolpi.ca/ergast/f1/");
    }

    public async Task<JolpicaDriverStandings> GetDriverStandingsAsync(CancellationToken ct = default)
    {
        var doc = await GetJsonAsync("current/driverStandings.json", ct);
        var list = doc.RootElement.GetProperty("MRData").GetProperty("StandingsTable")
            .GetProperty("StandingsLists")[0].GetProperty("DriverStandings");

        var season = int.Parse(doc.RootElement.GetProperty("MRData").GetProperty("StandingsTable").GetProperty("season").GetString()!);
        var round = int.Parse(doc.RootElement.GetProperty("MRData").GetProperty("StandingsTable").GetProperty("round").GetString()!);

        var drivers = list.EnumerateArray().Select(d => new JolpicaDriver(
            int.Parse(d.GetProperty("position").GetString()!),
            d.GetProperty("Driver").GetProperty("driverId").GetString()!,
            $"{d.GetProperty("Driver").GetProperty("givenName").GetString()} {d.GetProperty("Driver").GetProperty("familyName").GetString()}",
            d.GetProperty("Driver").GetProperty("code").GetString() ?? "",
            d.GetProperty("Constructors")[0].GetProperty("constructorId").GetString()!,
            d.GetProperty("Constructors")[0].GetProperty("name").GetString()!,
            int.Parse(d.GetProperty("points").GetString()!, CultureInfo.InvariantCulture),
            int.Parse(d.GetProperty("wins").GetString()!)
        )).ToList();

        return new JolpicaDriverStandings(season, round, drivers);
    }

    public async Task<IReadOnlyList<JolpicaConstructor>> GetConstructorStandingsAsync(CancellationToken ct = default)
    {
        var doc = await GetJsonAsync("current/constructorStandings.json", ct);
        var list = doc.RootElement.GetProperty("MRData").GetProperty("StandingsTable")
            .GetProperty("StandingsLists")[0].GetProperty("ConstructorStandings");

        return list.EnumerateArray().Select(c => new JolpicaConstructor(
            int.Parse(c.GetProperty("position").GetString()!),
            c.GetProperty("Constructor").GetProperty("constructorId").GetString()!,
            c.GetProperty("Constructor").GetProperty("name").GetString()!,
            int.Parse(c.GetProperty("points").GetString()!, CultureInfo.InvariantCulture),
            int.Parse(c.GetProperty("wins").GetString()!)
        )).ToList();
    }

    public async Task<JolpicaRace?> GetNextRaceAsync(CancellationToken ct = default)
    {
        var doc = await GetJsonAsync("current/next.json", ct);
        var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
        if (races.GetArrayLength() == 0) return null;
        return ParseRace(races[0]);
    }

    public async Task<IReadOnlyList<JolpicaRace>> GetSeasonScheduleAsync(int season, CancellationToken ct = default)
    {
        var doc = await GetJsonAsync($"{season}.json", ct);
        var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
        return races.EnumerateArray().Select(ParseRace).ToList();
    }

    public async Task<JolpicaRaceResult?> GetRaceResultAsync(int season, int round, CancellationToken ct = default)
    {
        try
        {
            var raceDoc = await GetJsonAsync($"{season}/{round}/results.json", ct);
            var races = raceDoc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
            if (races.GetArrayLength() == 0) return null;
            return await ParseRaceResultAsync(races[0], round, season, ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<JolpicaRaceResult>> GetRecentRacesAsync(int count, CancellationToken ct = default)
    {
        var standings = await GetDriverStandingsAsync(ct);
        var results = new List<JolpicaRaceResult>();

        for (var round = standings.Round; round >= 1 && results.Count < count; round--)
        {
            var result = await GetRaceResultAsync(standings.Season, round, ct);
            if (result is not null)
                results.Add(result);
        }

        return results;
    }

    private async Task<JolpicaRaceResult?> ParseRaceResultAsync(JsonElement race, int round, int season, CancellationToken ct)
    {
        var raceResults = race.GetProperty("Results").EnumerateArray().ToList();
        if (raceResults.Count == 0) return null;

        var winner = raceResults[0];
        var fastest = raceResults.OrderBy(r =>
            r.TryGetProperty("FastestLap", out var fl) && fl.TryGetProperty("rank", out var rank) &&
            rank.GetString() == "1" ? 0 : 1).First();

        string poleDriver = "?";
        try
        {
            var qualiDoc = await GetJsonAsync($"{season}/{round}/qualifying.json", ct);
            var qualiRace = qualiDoc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races")[0];
            var pole = qualiRace.GetProperty("QualifyingResults")[0];
            poleDriver = $"{pole.GetProperty("Driver").GetProperty("givenName").GetString()} {pole.GetProperty("Driver").GetProperty("familyName").GetString()}";
        }
        catch { /* qualifying optional */ }

        var podium = raceResults.Take(3).Select(r =>
            $"{r.GetProperty("Driver").GetProperty("givenName").GetString()} {r.GetProperty("Driver").GetProperty("familyName").GetString()}").ToList();

        var winnerName = $"{winner.GetProperty("Driver").GetProperty("givenName").GetString()} {winner.GetProperty("Driver").GetProperty("familyName").GetString()}";
        var fastestName = $"{fastest.GetProperty("Driver").GetProperty("givenName").GetString()} {fastest.GetProperty("Driver").GetProperty("familyName").GetString()}";

        var highlights = BuildHighlights(winnerName, poleDriver, fastestName, podium, raceResults);

        return new JolpicaRaceResult(
            race.GetProperty("raceName").GetString()!,
            race.GetProperty("Circuit").GetProperty("circuitId").GetString()!,
            DateTime.Parse(race.GetProperty("date").GetString()!),
            round,
            winnerName,
            poleDriver,
            fastestName,
            podium,
            highlights);
    }

    private static IReadOnlyList<string> BuildHighlights(
        string winner, string pole, string fastestLap, IReadOnlyList<string> podium,
        IReadOnlyList<JsonElement> results)
    {
        var highlights = new List<string>();

        if (pole != "?" && !string.Equals(pole, winner, StringComparison.OrdinalIgnoreCase))
            highlights.Add($"Pole ohne Sieg: {pole} startete von P1, gewann aber {winner}.");

        if (!string.Equals(fastestLap, winner, StringComparison.OrdinalIgnoreCase) && !podium.Contains(fastestLap))
            highlights.Add($"Schnellste Runde ging an {fastestLap} – ohne Podiumsplatz.");

        var comeback = results
            .Where(r => int.TryParse(r.GetProperty("grid").GetString(), out var g) && g > 10)
            .OrderBy(r => int.Parse(r.GetProperty("position").GetString()!))
            .FirstOrDefault();

        if (comeback.ValueKind != JsonValueKind.Undefined)
        {
            var pos = comeback.GetProperty("position").GetString();
            var grid = comeback.GetProperty("grid").GetString();
            var name = $"{comeback.GetProperty("Driver").GetProperty("givenName").GetString()} {comeback.GetProperty("Driver").GetProperty("familyName").GetString()}";
            if (int.Parse(pos!) <= 3)
                highlights.Add($"Comeback: {name} von P{grid} auf P{pos}.");
        }

        if (highlights.Count == 0)
            highlights.Add($"Dominanz: {winner} holte den Sieg vor {string.Join(" und ", podium.Skip(1))}.");

        return highlights.Take(2).ToList();
    }

    public async Task<Dictionary<string, string>> GetDriverFormAsync(int season, int fromRound, int toRound, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>();

        for (var round = fromRound; round <= toRound; round++)
        {
            try
            {
                var doc = await GetJsonAsync($"{season}/{round}/results.json", ct);
                var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
                if (races.GetArrayLength() == 0) continue;

                foreach (var result in races[0].GetProperty("Results").EnumerateArray())
                {
                    var id = result.GetProperty("Driver").GetProperty("driverId").GetString()!;
                    var pos = int.Parse(result.GetProperty("position").GetString()!);
                    var charForm = pos == 1 ? 'W' : pos <= 3 ? 'P' : 'L';
                    form[id] = (form.GetValueOrDefault(id) ?? "") + charForm;
                }
            }
            catch { /* skip round */ }
        }

        return form;
    }

    public async Task<Dictionary<string, int>> GetPointsGainedLastRacesAsync(int season, int currentRound, int raceCount, CancellationToken ct = default)
    {
        var points = new Dictionary<string, int>();
        var start = Math.Max(1, currentRound - raceCount + 1);

        for (var round = start; round <= currentRound; round++)
        {
            try
            {
                var doc = await GetJsonAsync($"{season}/{round}/results.json", ct);
                var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
                if (races.GetArrayLength() == 0) continue;

                foreach (var result in races[0].GetProperty("Results").EnumerateArray())
                {
                    var id = result.GetProperty("Driver").GetProperty("driverId").GetString()!;
                    var pts = int.Parse(result.GetProperty("points").GetString()!);
                    points[id] = points.GetValueOrDefault(id) + pts;
                }
            }
            catch { /* skip */ }
        }

        return points;
    }

    public async Task<Dictionary<string, int>> GetPodiumCountsAsync(int season, int rounds, CancellationToken ct = default)
    {
        var podiums = new Dictionary<string, int>();
        for (var round = 1; round <= rounds; round++)
        {
            try
            {
                var doc = await GetJsonAsync($"{season}/{round}/results.json", ct);
                var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
                if (races.GetArrayLength() == 0) continue;

                foreach (var result in races[0].GetProperty("Results").EnumerateArray().Take(3))
                {
                    var id = result.GetProperty("Driver").GetProperty("driverId").GetString()!;
                    podiums[id] = podiums.GetValueOrDefault(id) + 1;
                }
            }
            catch { /* skip */ }
        }
        return podiums;
    }

    public async Task<Dictionary<string, int>> GetHistoricalWinsAtCircuitAsync(string circuitId, int years, CancellationToken ct = default)
    {
        var wins = new Dictionary<string, int>();
        var currentYear = DateTime.UtcNow.Year;

        for (var year = currentYear - years; year < currentYear; year++)
        {
            try
            {
                var doc = await GetJsonAsync($"{year}/circuits/{circuitId}/results.json", ct);
                var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
                if (races.GetArrayLength() == 0) continue;

                var winner = races[0].GetProperty("Results")[0];
                var id = winner.GetProperty("Driver").GetProperty("driverId").GetString()!;
                wins[id] = wins.GetValueOrDefault(id) + 1;
            }
            catch { /* skip year */ }
        }

        return wins;
    }

    public async Task<Dictionary<string, IReadOnlyList<string>>> GetDriverLastResultsAsync(
        int season, int currentRound, int raceCount, CancellationToken ct = default)
    {
        var results = new Dictionary<string, List<string>>();
        var start = Math.Max(1, currentRound - raceCount + 1);

        for (var round = start; round <= currentRound; round++)
        {
            try
            {
                var doc = await GetJsonAsync($"{season}/{round}/results.json", ct);
                var races = doc.RootElement.GetProperty("MRData").GetProperty("RaceTable").GetProperty("Races");
                if (races.GetArrayLength() == 0) continue;

                var raceName = races[0].GetProperty("raceName").GetString()!.Replace(" Grand Prix", " GP");

                foreach (var result in races[0].GetProperty("Results").EnumerateArray())
                {
                    var id = result.GetProperty("Driver").GetProperty("driverId").GetString()!;
                    var pos = result.GetProperty("position").GetString();
                    if (!results.ContainsKey(id)) results[id] = [];
                    results[id].Add($"P{pos} {raceName}");
                }
            }
            catch { /* skip */ }
        }

        return results.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<string>)kv.Value);
    }

    private static JolpicaRace ParseRace(JsonElement race)
    {
        var circuit = race.GetProperty("Circuit");
        var loc = circuit.GetProperty("Location");
        var dateStr = race.GetProperty("date").GetString()!;
        var timeStr = race.TryGetProperty("time", out var t) ? t.GetString() : "12:00:00Z";
        var raceDate = DateTime.Parse($"{dateStr}T{timeStr}", null, DateTimeStyles.AssumeUniversal);

        return new JolpicaRace(
            race.GetProperty("raceName").GetString()!,
            circuit.GetProperty("circuitId").GetString()!,
            circuit.GetProperty("circuitName").GetString()!,
            circuit.TryGetProperty("url", out var curl) ? curl.GetString() : null,
            loc.GetProperty("country").GetString()!,
            loc.GetProperty("locality").GetString()!,
            double.Parse(loc.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture),
            double.Parse(loc.GetProperty("long").GetString()!, CultureInfo.InvariantCulture),
            int.Parse(race.GetProperty("season").GetString()!),
            int.Parse(race.GetProperty("round").GetString()!),
            raceDate);
    }

    private async Task<JsonDocument> GetJsonAsync(string path, CancellationToken ct)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
    }
}

public record JolpicaDriver(int Position, string DriverId, string Name, string Code, string TeamId, string TeamName, int Points, int Wins);
public record JolpicaDriverStandings(int Season, int Round, IReadOnlyList<JolpicaDriver> Drivers);
public record JolpicaConstructor(int Position, string ConstructorId, string Name, int Points, int Wins);
public record JolpicaRace(string RaceName, string CircuitId, string CircuitName, string? CircuitWikiUrl, string Country, string Locality, double Lat, double Lon, int Season, int Round, DateTime RaceDateTime);
public record JolpicaRaceResult(string RaceName, string CircuitId, DateTime Date, int Round, string Winner, string Pole, string FastestLap, IReadOnlyList<string> Podium, IReadOnlyList<string> Highlights);
