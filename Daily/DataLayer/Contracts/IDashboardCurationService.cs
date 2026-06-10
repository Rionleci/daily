using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IDashboardCurationService
{
    Task<WeatherSnapshot> GetWeatherAsync(CancellationToken ct = default);

    IReadOnlyList<QuickStatusCard> BuildQuickStatus(
        IReadOnlyList<MarketQuote> quotes,
        F1Teaser f1,
        IReadOnlyList<TechNews> tech);

    IReadOnlyList<TodayImportantCard> BuildTodayImportant(
        IReadOnlyList<MarketNews> marketNews,
        IReadOnlyList<PoliticsNews> politics,
        IReadOnlyList<TechNews> tech,
        IReadOnlyList<MarketQuote> quotes,
        F1Teaser f1);

    StockOfTheDay BuildStockOfTheDay(IReadOnlyList<MarketMover> gainers, IReadOnlyList<MarketQuote> quotes);

    CompanyOfTheDay GetCompanyOfTheDay();

    IReadOnlyList<QuickLink> GetQuickLinks();

    DailyLearnTerm GetDailyLearnTerm();

    IReadOnlyList<DashboardEvent> BuildTopEvents(
        IReadOnlyList<MarketNews> marketNews,
        IReadOnlyList<PoliticsNews> politics,
        IReadOnlyList<TechNews> tech,
        F1Brief f1);

    WeekOverview BuildWeekOverview(F1Brief f1, WeatherSnapshot? weather);
}
