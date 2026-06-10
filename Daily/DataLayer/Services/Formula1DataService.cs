using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

public class Formula1DataService : IFormula1DataService
{
    private readonly JolpicaF1Client _jolpica;
    private readonly OpenF1Client _openF1;
    private readonly WikipediaCircuitClient _wikipedia;
    private readonly MultiviewerCircuitClient _multiviewer;
    private readonly OpenMeteoClient _weather;
    private readonly F1NewsRssClient _news;
    private readonly F1ImageResolver _images;
    private readonly IMemoryCache _cache;

    public Formula1DataService(
        JolpicaF1Client jolpica,
        OpenF1Client openF1,
        WikipediaCircuitClient wikipedia,
        MultiviewerCircuitClient multiviewer,
        OpenMeteoClient weather,
        F1NewsRssClient news,
        F1ImageResolver images,
        IMemoryCache cache)
    {
        _jolpica = jolpica;
        _openF1 = openF1;
        _wikipedia = wikipedia;
        _multiviewer = multiviewer;
        _weather = weather;
        _news = news;
        _images = images;
        _cache = cache;
    }

    public async Task<Formula1CenterData> GetCenterDataAsync(CancellationToken ct = default)
    {
        try
        {
            return await _cache.GetOrCreateAsync("f1_center", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await BuildAsync(ct);
            }) ?? EmptyError("Keine Daten verfügbar.");
        }
        catch (Exception ex)
        {
            return EmptyError($"F1-Daten konnten nicht geladen werden: {ex.Message}");
        }
    }

    private static Formula1CenterData EmptyError(string message) => new(
        null, [], [], null, null, [], [], [], [], null, [], DateTime.UtcNow, message);

    private async Task<Formula1CenterData> BuildAsync(CancellationToken ct)
    {
        var standings = await _jolpica.GetDriverStandingsAsync(ct);
        var constructors = await _jolpica.GetConstructorStandingsAsync(ct);
        var nextRace = await _jolpica.GetNextRaceAsync(ct);
        var schedule = await _jolpica.GetSeasonScheduleAsync(standings.Season, ct);
        var meetings = await _openF1.GetMeetingsForYearAsync(standings.Season, ct);
        var recentRaces = await _jolpica.GetRecentRacesAsync(8, ct);
        var driverLookup = await _images.GetDriverLookupAsync(ct);

        var seasonInfo = new F1SeasonInfo(
            standings.Season,
            standings.Round,
            schedule.Count,
            Math.Max(0, schedule.Count - standings.Round));

        var formFrom = Math.Max(1, standings.Round - 4);
        var form = await _jolpica.GetDriverFormAsync(standings.Season, formFrom, standings.Round, ct);
        var pointsLast3 = await _jolpica.GetPointsGainedLastRacesAsync(standings.Season, standings.Round, 3, ct);
        var podiums = await _jolpica.GetPodiumCountsAsync(standings.Season, standings.Round, ct);
        var lastResults = await _jolpica.GetDriverLastResultsAsync(standings.Season, standings.Round, 3, ct);

        var resultByRound = recentRaces.ToDictionary(r => r.Round);
        var (upcoming, past) = await BuildCalendarAsync(schedule, meetings, standings.Round, resultByRound, ct);

        var hero = nextRace is not null ? await BuildHeroAsync(nextRace, meetings, ct) : null;
        var titleFight = BuildTitleFight(standings, pointsLast3, form, driverLookup);
        var teams = BuildTeams(constructors, driverLookup);
        var drivers = BuildDrivers(standings, form, podiums, lastResults, driverLookup);
        var recent = BuildRecentRaces(recentRaces.Take(5).ToList());
        var news = await BuildNewsAsync(ct);

        return new Formula1CenterData(
            seasonInfo, upcoming, past, hero, titleFight, teams, drivers, recent, news, null,
            [], DateTime.UtcNow);
    }

    private async Task<(IReadOnlyList<F1CalendarRace> Upcoming, IReadOnlyList<F1CalendarRace> Past)> BuildCalendarAsync(
        IReadOnlyList<JolpicaRace> schedule,
        IReadOnlyList<OpenF1Meeting> meetings,
        int currentRound,
        Dictionary<int, JolpicaRaceResult> resultByRound,
        CancellationToken ct)
    {
        var culture = new System.Globalization.CultureInfo("de-CH");
        var now = DateTime.UtcNow;
        var upcoming = new List<F1CalendarRace>();
        var past = new List<F1CalendarRace>();

        foreach (var race in schedule)
        {
            var meeting = FindMeeting(meetings, race);
            var dateDisplay = race.RaceDateTime.ToLocalTime().ToString("ddd, d. MMM · HH:mm", culture);
            var isPast = race.Round <= currentRound;

            JolpicaRaceResult? result = null;
            if (isPast)
            {
                result = resultByRound.GetValueOrDefault(race.Round)
                         ?? await _jolpica.GetRaceResultAsync(race.Season, race.Round, ct);
            }

            var circuitImage = F1MediaHelper.ResolveCircuitImage(
                meeting?.CircuitImageUrl, race.CircuitId, race.CircuitName);

            var entry = new F1CalendarRace(
                race.Round,
                race.RaceName,
                race.RaceName.Replace(" Grand Prix", " GP"),
                race.CircuitName,
                race.Country,
                meeting?.CountryFlagUrl,
                circuitImage,
                race.RaceDateTime,
                dateDisplay,
                isPast && result is not null,
                result?.Winner,
                result?.Pole,
                result?.FastestLap,
                result?.Podium,
                result?.Highlights,
                race.CircuitId);

            if (isPast && result is not null)
                past.Add(entry);
            else if (!isPast || race.RaceDateTime > now)
                upcoming.Add(entry);
        }

        return (
            upcoming.OrderBy(r => r.Round).ToList(),
            past.OrderByDescending(r => r.Round).Take(6).ToList());
    }

    private static OpenF1Meeting? FindMeeting(IReadOnlyList<OpenF1Meeting> meetings, JolpicaRace race)
    {
        var normalizedId = race.CircuitId.Replace("_", "").ToLowerInvariant();
        var normalizedName = race.CircuitName.ToLowerInvariant();

        return meetings.FirstOrDefault(m =>
            m.CircuitShortName.Replace(" ", "").ToLowerInvariant().Contains(normalizedId) ||
            normalizedName.Contains(m.CircuitShortName.ToLowerInvariant()) ||
            normalizedName.Contains(m.Location.ToLowerInvariant()) ||
            m.MeetingName.Contains(race.RaceName.Replace(" Grand Prix", ""), StringComparison.OrdinalIgnoreCase));
    }

    private async Task<F1HeroRace> BuildHeroAsync(JolpicaRace race, IReadOnlyList<OpenF1Meeting> meetings, CancellationToken ct)
    {
        var meeting = FindMeeting(meetings, race)
                      ?? await _openF1.GetNextMeetingAsync(ct);

        var wikiInfo = await _wikipedia.GetCircuitInfoAsync(race.CircuitName, race.CircuitWikiUrl, ct);

        var lengthKm = wikiInfo.LengthKm;
        if (lengthKm == 0 && meeting is not null)
            lengthKm = await _multiviewer.GetTrackLengthKmAsync(meeting.CircuitInfoUrl, ct) ?? 0;

        var laps = wikiInfo.Laps;
        if (laps == 0 && lengthKm > 0)
            laps = (int)Math.Round(305 / lengthKm);

        var (temp, weatherDesc) = await _weather.GetCurrentWeatherAsync(race.Lat, race.Lon, ct);
        var countdown = (long)Math.Max(0, (race.RaceDateTime - DateTime.UtcNow).TotalSeconds);

        var circuitImage = F1MediaHelper.ResolveCircuitImage(
            meeting?.CircuitImageUrl, race.CircuitId, race.CircuitName);
        var countryFlagUrl = meeting?.CountryFlagUrl;
        var countryCode = meeting?.CountryCode ?? "";
        var flagEmoji = !string.IsNullOrEmpty(countryFlagUrl) ? "" : F1Catalog.GetFlag(race.Country);

        return new F1HeroRace(
            race.RaceName,
            race.CircuitName,
            race.Country,
            countryCode,
            flagEmoji,
            countryFlagUrl,
            race.RaceDateTime,
            countdown,
            lengthKm,
            laps,
            lengthKm > 0 && laps > 0 ? lengthKm * laps : 0,
            weatherDesc,
            temp,
            circuitImage,
            circuitImage,
            race.Lat,
            race.Lon);
    }

    private static F1TitleFight? BuildTitleFight(
        JolpicaDriverStandings standings,
        Dictionary<string, int> pointsLast3,
        Dictionary<string, string> form,
        Dictionary<string, OpenF1Driver> driverLookup)
    {
        var top = standings.Drivers.Take(2).ToList();
        if (top.Count < 2) return null;

        var leader = top[0];
        var challenger = top[1];
        var maxPts = leader.Points;

        var momentum = pointsLast3.OrderByDescending(kv => kv.Value).FirstOrDefault();
        var momentumDriver = standings.Drivers.FirstOrDefault(d => d.DriverId == momentum.Key);
        var momentumName = momentumDriver?.Name ?? leader.Name;
        var momentumPts = momentum.Value;

        return new F1TitleFight(
            ToFightEntry(leader, pointsLast3, form, maxPts, driverLookup),
            ToFightEntry(challenger, pointsLast3, form, maxPts, driverLookup),
            leader.Points - challenger.Points,
            momentumName,
            momentumPts > 0
                ? $"{momentumName} holte in den letzten 3 Rennen {momentumPts} Punkte – aktuell das stärkere Momentum."
                : "Der Titelkampf bleibt offen – beide Fahrer liefern sich ein enges Duell.");
    }

    private static F1TitleFightEntry ToFightEntry(
        JolpicaDriver d, Dictionary<string, int> pts, Dictionary<string, string> form,
        int maxPts, Dictionary<string, OpenF1Driver> lookup)
    {
        var color = lookup.TryGetValue(d.Code, out var o1) ? $"#{o1.TeamColour}" : F1Catalog.GetTeam(d.TeamId).Color;
        return new(d.Position, d.Name, d.TeamName, color, d.Points,
            pts.GetValueOrDefault(d.DriverId),
            form.GetValueOrDefault(d.DriverId, "-----").PadRight(5, '-')[..5],
            maxPts > 0 ? (double)d.Points / maxPts * 100 : 0);
    }

    private static IReadOnlyList<F1TeamCard> BuildTeams(
        IReadOnlyList<JolpicaConstructor> constructors,
        Dictionary<string, OpenF1Driver> driverLookup)
    {
        var teamColours = driverLookup.Values
            .GroupBy(d => d.TeamName)
            .ToDictionary(g => g.Key, g => $"#{g.First().TeamColour}");

        return constructors.Select(c =>
        {
            var meta = F1Catalog.GetTeam(c.ConstructorId);
            var color = teamColours.GetValueOrDefault(c.Name, meta.Color);
            return new F1TeamCard(c.Position, c.ConstructorId, c.Name, color, meta.LogoUrl, c.Points, c.Wins);
        }).ToList();
    }

    private IReadOnlyList<F1DriverCard> BuildDrivers(
        JolpicaDriverStandings standings,
        Dictionary<string, string> form,
        Dictionary<string, int> podiums,
        Dictionary<string, IReadOnlyList<string>> lastResults,
        Dictionary<string, OpenF1Driver> driverLookup) =>
        standings.Drivers.Select(d =>
        {
            var portrait = _images.ResolveHeadshot(d, driverLookup);
            var teamColor = _images.ResolveTeamColour(d, driverLookup);
            return new F1DriverCard(
                d.Position, d.DriverId, d.Name, d.Code, d.TeamName, teamColor,
                portrait,
                d.Points, d.Wins,
                podiums.GetValueOrDefault(d.DriverId),
                form.GetValueOrDefault(d.DriverId, "-----").PadRight(5, '-')[..5],
                lastResults.GetValueOrDefault(d.DriverId, []));
        }).ToList();

    private static IReadOnlyList<F1RecentRaceCard> BuildRecentRaces(IReadOnlyList<JolpicaRaceResult> races) =>
        races.Select(r => new F1RecentRaceCard(
            r.RaceName,
            r.RaceName.Replace(" Grand Prix", " GP"),
            r.Date.ToString("dd.MM.yyyy"),
            r.Winner,
            r.Pole,
            r.FastestLap,
            r.Podium,
            r.CircuitId)).ToList();

    private async Task<IReadOnlyList<F1NewsStory>> BuildNewsAsync(CancellationToken ct)
    {
        var raw = await _news.FetchNewsAsync(6, ct);
        return _news.EnrichNews(raw);
    }

    private async Task<F1PredictionsSection> BuildPredictionsAsync(
        JolpicaDriverStandings standings,
        JolpicaRace nextRace,
        Dictionary<string, int> pointsLast3,
        CancellationToken ct)
    {
        var historical = await _jolpica.GetHistoricalWinsAtCircuitAsync(nextRace.CircuitId, 5, ct);
        var scores = new Dictionary<string, double>();

        foreach (var d in standings.Drivers)
        {
            var score = d.Points * 0.4 + pointsLast3.GetValueOrDefault(d.DriverId) * 2.0 +
                        historical.GetValueOrDefault(d.DriverId) * 15;
            scores[d.DriverId] = score;
        }

        var total = scores.Values.Sum();
        var top4 = scores.OrderByDescending(kv => kv.Value).Take(4).ToList();
        var predictions = top4.Select((kv, i) =>
        {
            var driver = standings.Drivers.First(d => d.DriverId == kv.Key);
            var pct = total > 0 ? (int)Math.Round(kv.Value / total * 100) : 25;
            return new F1RacePrediction(i + 1, driver.Name, driver.TeamName, pct);
        }).ToList();

        var wikiInfo = await _wikipedia.GetCircuitInfoAsync(nextRace.CircuitName, nextRace.CircuitWikiUrl, ct);
        var histWinner = historical.OrderByDescending(kv => kv.Value).FirstOrDefault();
        var histName = histWinner.Key is not null
            ? standings.Drivers.FirstOrDefault(d => d.DriverId == histWinner.Key)?.Name
            : null;

        var lengthStr = wikiInfo.LengthKm > 0 ? $"{wikiInfo.LengthKm} km" : "der Strecke";
        var lapsStr = wikiInfo.Laps > 0 ? $", {wikiInfo.Laps} Runden" : "";

        var reasoning = histName is not null
            ? $"{histName} war auf {nextRace.CircuitName} in den letzten Jahren stark. " +
              $"Aktuell führt {standings.Drivers[0].Name} die WM mit {standings.Drivers[0].Points} Punkten an."
            : $"Auf {nextRace.CircuitName} ({lengthStr}{lapsStr}) " +
              $"führt {standings.Drivers[0].Name} die WM an. Form der letzten Rennen fliesst in die Prognose ein.";

        return new F1PredictionsSection(predictions, reasoning);
    }
}
