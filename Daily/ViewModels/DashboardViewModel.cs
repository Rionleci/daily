using Daily.Models;

namespace Daily.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = "Rion";
    public string Greeting { get; set; } = "";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public WeatherSnapshot Weather { get; set; } = null!;
    public IReadOnlyList<QuickStatusCard> QuickStatus { get; set; } = [];
    public IReadOnlyList<TodayImportantCard> TodayImportant { get; set; } = [];
    public F1Teaser F1Teaser { get; set; } = null!;
    public IReadOnlyList<DashboardEvent> TopEvents { get; set; } = [];
    public StockOfTheDay StockOfTheDay { get; set; } = null!;
    public CompanyOfTheDay CompanyOfTheDay { get; set; } = null!;
    public DailyLearnTerm DailyLearn { get; set; } = null!;
    public IReadOnlyList<QuickLink> QuickLinks { get; set; } = [];
    public WeekOverview WeekOverview { get; set; } = null!;
    public IReadOnlyList<MarketQuote> MarketQuotes { get; set; } = [];
    public IReadOnlyList<MarketMover> TopGainers { get; set; } = [];
    public IReadOnlyList<MarketMover> TopLosers { get; set; } = [];
    public IReadOnlyList<MarketNews> MarketNews { get; set; } = [];
    public IReadOnlyList<PoliticsNews> PoliticsNews { get; set; } = [];
    public IReadOnlyList<TechNews> TechNews { get; set; } = [];
    public IReadOnlyList<EntrepreneurNews> EntrepreneurNews { get; set; } = [];
    public IReadOnlyList<DevTechItem> DevTech { get; set; } = [];
    public IReadOnlyList<WatchlistItem> Watchlist { get; set; } = [];
    public IReadOnlyList<GlossaryTerm> GlossaryTerms { get; set; } = [];
}

public class EventDetailViewModel
{
    public TodayImportantCard Event { get; set; } = null!;
    public string Greeting { get; set; } = "";
}
