using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

/// <summary>
/// Leichtgewichtiger F1-Abruf nur für Dashboard-Hero (ohne Formula1DataService).
/// </summary>
public class F1BriefService : IF1BriefService
{
    private readonly JolpicaF1Client _jolpica;
    private readonly IMemoryCache _cache;

    public F1BriefService(JolpicaF1Client jolpica, IMemoryCache cache)
    {
        _jolpica = jolpica;
        _cache = cache;
    }

    public async Task<F1Brief> GetNextRaceBriefAsync(CancellationToken ct = default)
    {
        try
        {
            var brief = await _cache.GetOrCreateAsync("f1_brief_next", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                var race = await _jolpica.GetNextRaceAsync(ct);
                if (race is null) return DemoDataProvider.F1BriefFallback;

                var countdown = (long)Math.Max(0, (race.RaceDateTime - DateTime.UtcNow).TotalSeconds);
                return new F1Brief(
                    race.RaceName,
                    race.CircuitName,
                    race.Country,
                    race.RaceDateTime,
                    countdown,
                    "https://www.formula1.com/en/racing/2026.html");
            });
            return brief ?? DemoDataProvider.F1BriefFallback;
        }
        catch
        {
            return DemoDataProvider.F1BriefFallback;
        }
    }

    public async Task<F1Teaser> GetTeaserAsync(CancellationToken ct = default)
    {
        try
        {
            var teaser = await _cache.GetOrCreateAsync("f1_teaser", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var raceTask = _jolpica.GetNextRaceAsync(ct);
                var standingsTask = _jolpica.GetDriverStandingsAsync(ct);
                await Task.WhenAll(raceTask, standingsTask);

                var race = await raceTask;
                var standings = await standingsTask;

                if (race is null)
                    return DemoDataProvider.F1TeaserFallback;

                var countdown = (long)Math.Max(0, (race.RaceDateTime - DateTime.UtcNow).TotalSeconds);
                var days = (int)Math.Ceiling(countdown / 86400.0);
                var top3 = standings.Drivers
                    .OrderBy(d => d.Position)
                    .Take(3)
                    .Select(d => new F1StandingLine(d.Position, d.Name))
                    .ToList();

                if (top3.Count == 0)
                    top3 = DemoDataProvider.F1TeaserFallback.Standings.ToList();

                return new F1Teaser(
                    race.RaceName.Replace(" Grand Prix", " GP"),
                    Math.Max(0, days),
                    countdown,
                    top3,
                    "#f1");
            });
            return teaser ?? DemoDataProvider.F1TeaserFallback;
        }
        catch
        {
            return DemoDataProvider.F1TeaserFallback;
        }
    }
}
