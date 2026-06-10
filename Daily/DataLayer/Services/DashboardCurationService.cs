using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

public class DashboardCurationService : IDashboardCurationService
{
    private readonly OpenMeteoClient _weather;
    private readonly IMemoryCache _cache;

    public DashboardCurationService(OpenMeteoClient weather, IMemoryCache cache)
    {
        _weather = weather;
        _cache = cache;
    }

    public async Task<WeatherSnapshot> GetWeatherAsync(CancellationToken ct = default)
    {
        try
        {
            return await _cache.GetOrCreateAsync("dashboard_weather", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return await _weather.GetWeatherSnapshotAsync(ct: ct);
            }) ?? DemoDataProvider.DefaultWeather;
        }
        catch
        {
            return DemoDataProvider.DefaultWeather;
        }
    }

    public IReadOnlyList<QuickStatusCard> BuildQuickStatus(
        IReadOnlyList<MarketQuote> quotes,
        F1Teaser f1,
        IReadOnlyList<TechNews> tech)
    {
        var smi = quotes.FirstOrDefault(q => q.Symbol is "^SSMI" or "SMI");
        var btc = quotes.FirstOrDefault(q => q.Symbol == "BTC");

        var cards = new List<QuickStatusCard>();

        if (smi is not null)
        {
            cards.Add(new("📈", "SMI",
                $"{(smi.ChangeTodayPercent >= 0 ? "+" : "")}{smi.ChangeTodayPercent:N1} %",
                smi.Name, smi.ChangeTodayPercent >= 0));
        }

        if (btc is not null)
        {
            cards.Add(new("₿", "Bitcoin",
                $"{(btc.ChangeTodayPercent >= 0 ? "+" : "")}{btc.ChangeTodayPercent:N1} %",
                $"{btc.Price:N0} USD", btc.ChangeTodayPercent >= 0));
        }

        cards.Add(new("🏎️", "Formel 1",
            f1.RaceName,
            f1.DaysLeft > 0 ? $"Noch {f1.DaysLeft} Tage" : "Heute"));

        var aiCount = Math.Min(tech.Count, 3);
        cards.Add(new("🤖", "AI",
            aiCount > 0 ? $"{aiCount} wichtige Entwicklungen" : "Keine News",
            tech.FirstOrDefault()?.Topic ?? "Tech"));

        cards.Add(new("🏠", "Immobilien CH",
            "Markt stabil",
            "Zürich & SNB-Zinsen"));

        return cards;
    }

    public IReadOnlyList<TodayImportantCard> BuildTodayImportant(
        IReadOnlyList<MarketNews> marketNews,
        IReadOnlyList<PoliticsNews> politics,
        IReadOnlyList<TechNews> tech,
        IReadOnlyList<MarketQuote> quotes,
        F1Teaser f1)
    {
        var items = new List<TodayImportantCard>();

        if (marketNews.Count > 0)
        {
            var n = marketNews[0];
            items.Add(new("nvidia", "🔥", n.Title, "Börse", n.Summary, n.Summary, n.WhyImportant, n.SourceUrl));
        }

        if (politics.Count > 0)
        {
            var n = politics[0];
            items.Add(new("snb", "🏦", n.Title, "Wirtschaft", n.Summary, n.Summary,
                n.EconomicImpact.FirstOrDefault(), n.SourceUrl));
        }

        items.Add(new("f1-upgrade", "🏎️", "McLaren Upgrade vor Barcelona",
            "Formel 1", "Neues Aerodynamik-Paket im WM-Kampf.",
            DemoDataProvider.F1NewsList[0].Summary,
            DemoDataProvider.F1NewsList[0].WhatItMeans, "https://www.formula1.com/"));

        if (tech.Count > 0)
        {
            var n = tech.FirstOrDefault(t => t.Title.Contains("OpenAI", StringComparison.OrdinalIgnoreCase)) ?? tech[0];
            items.Add(new("openai", "🤖", n.Title, "Tech", n.Summary, n.Summary,
                n.DeveloperRelevance, n.SourceUrl));
        }

        var btc = quotes.FirstOrDefault(q => q.Symbol == "BTC");
        if (btc is not null)
        {
            items.Add(new("btc", "₿", $"Bitcoin über {btc.Price:N0} USD",
                "Krypto", $"Bitcoin {(btc.ChangeTodayPercent >= 0 ? "steigt" : "fällt")} um {Math.Abs(btc.ChangeTodayPercent):N1} %.",
                "Krypto bleibt ein Sentiment-Barometer für Risiko-Appetit an den Märkten.",
                "Relevant für Diversifikation und Makro-Stimmung.", null));
        }

        items.Add(new("immobilien", "🏠", "Immobilienmarkt Zürich",
            "Immobilien", "Knappes Angebot hält Preise unter Druck.",
            DemoDataProvider.RealEstateInsight,
            "SNB-Zinsen beeinflussen Hypotheken direkt.", "https://www.wohnungsboerse.net/"));

        return items.Take(6).ToList();
    }

    public StockOfTheDay BuildStockOfTheDay(
        IReadOnlyList<MarketMover> gainers,
        IReadOnlyList<MarketQuote> quotes)
    {
        var mover = gainers.FirstOrDefault(g => g.Symbol == "NVDA") ?? gainers.FirstOrDefault();
        if (mover is null) return DemoDataProvider.StockOfTheDay;

        return DemoDataProvider.StockOfTheDay with
        {
            Price = mover.Price,
            ChangePercent = mover.ChangePercent
        };
    }

    public CompanyOfTheDay GetCompanyOfTheDay()
    {
        var companies = DemoDataProvider.CompaniesOfTheDay;
        return companies[DateTime.Now.DayOfYear % companies.Count];
    }

    public IReadOnlyList<QuickLink> GetQuickLinks() => DemoDataProvider.QuickLinks;

    public DailyLearnTerm GetDailyLearnTerm()
    {
        var terms = DemoDataProvider.InvestGlossary;
        var term = terms[DateTime.Now.DayOfYear % terms.Count];
        return new DailyLearnTerm(
            term.Term, term.Slug, term.SimpleExplanation,
            term.WhyImportant, term.PracticalExample, 55);
    }

    public IReadOnlyList<DashboardEvent> BuildTopEvents(
        IReadOnlyList<MarketNews> marketNews,
        IReadOnlyList<PoliticsNews> politics,
        IReadOnlyList<TechNews> tech,
        F1Brief f1)
    {
        var events = new List<DashboardEvent>();

        if (marketNews.Count > 0)
        {
            var n = marketNews[0];
            events.Add(new(1, n.Title, "Börse", n.Summary, n.WhyImportant, n.SourceUrl));
        }

        if (politics.Count > 0)
        {
            var n = politics[0];
            events.Add(new(2, n.Title, "Wirtschaft", n.Summary,
                n.EconomicImpact.FirstOrDefault() ?? "Relevant für Zinsen und Märkte.", n.SourceUrl));
        }

        if (politics.Count > 1)
        {
            var n = politics[1];
            events.Add(new(3, n.Title, "Politik", n.Summary,
                string.Join(" ", n.EconomicImpact.Take(2)), n.SourceUrl));
        }
        else if (politics.Count > 0)
        {
            var n = politics[0];
            events.Add(new(3, "Globale Wirtschaft im Fokus", "Politik",
                "Internationale Handels- und Zinspolitik prägen die Märkte.",
                n.EconomicImpact.LastOrDefault() ?? "Diversifikation wichtiger denn je.", n.SourceUrl));
        }

        if (tech.Count > 0)
        {
            var n = tech[0];
            events.Add(new(4, n.Title, "Tech", n.Summary, n.EntrepreneurRelevance, n.SourceUrl));
        }

        events.Add(new(5, f1.RaceName, "Formel 1",
            $"Nächstes Rennen: {f1.CircuitName}, {f1.Country}.",
            "WM-Kampf und Upgrades könnten die Saison entscheiden.", f1.SourceUrl));

        return events.Take(5).ToList();
    }

    public WeekOverview BuildWeekOverview(F1Brief f1, WeatherSnapshot? weather)
    {
        var events = new List<WeekEvent>(DemoDataProvider.WeekEvents);

        var raceDay = f1.RaceDateTime.ToLocalTime();
        var qualiDay = raceDay.AddDays(-1);
        var culture = new System.Globalization.CultureInfo("de-CH");

        events.Add(new WeekEvent(
            qualiDay.ToString("dddd", culture),
            qualiDay.ToString("d. MMM", culture),
            $"{f1.RaceName.Replace(" Grand Prix", " GP")} Qualifying",
            "Formel 1", "bi-flag-fill", f1.SourceUrl));

        events.Add(new WeekEvent(
            raceDay.ToString("dddd", culture),
            raceDay.ToString("d. MMM", culture),
            f1.RaceName,
            "Formel 1", "bi-trophy-fill", f1.SourceUrl));

        var weatherTrend = weather is not null && weather.Forecast.Count >= 3
            ? $"Von {weather.Forecast[0].HighC:N0}°C auf {weather.Forecast[^1].HighC:N0}°C – {weather.Forecast[^1].Description}."
            : "Wechselhaft mit sonnigen Abschnitten.";

        var f1Summary = $"Nächstes Rennen: {f1.RaceName} in {f1.Country}.";

        return new WeekOverview(events, weatherTrend, f1Summary);
    }
}
