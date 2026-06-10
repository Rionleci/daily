using Daily.DataLayer.Contracts;
using Daily.Models;
using Daily.ViewModels;

namespace Daily.DataLayer.Services;

public class DashboardService : IDashboardService
{
    private readonly IMarketDataService _markets;
    private readonly INewsDataService _news;
    private readonly IGlossaryService _glossary;
    private readonly IDashboardCurationService _curation;
    private readonly IF1BriefService _f1Brief;

    public DashboardService(
        IMarketDataService markets,
        INewsDataService news,
        IGlossaryService glossary,
        IDashboardCurationService curation,
        IF1BriefService f1Brief)
    {
        _markets = markets;
        _news = news;
        _glossary = glossary;
        _curation = curation;
        _f1Brief = f1Brief;
    }

    public async Task<DashboardViewModel> BuildDashboardAsync(string userName = "Rion", CancellationToken ct = default)
    {
        var quotesTask = ApiLoadHelper.SafeAsync(_markets.GetQuotesAsync, DemoDataProvider.MarketQuotes, ct);
        var gainersTask = ApiLoadHelper.SafeAsync(_markets.GetTopGainersAsync, DemoDataProvider.TopGainers, ct);
        var losersTask = ApiLoadHelper.SafeAsync(_markets.GetTopLosersAsync, DemoDataProvider.TopLosers, ct);
        var marketNewsTask = ApiLoadHelper.SafeAsync(_markets.GetMarketNewsAsync, DemoDataProvider.MarketNewsList, ct);
        var politicsTask = ApiLoadHelper.SafeAsync(_news.GetPoliticsNewsAsync, DemoDataProvider.PoliticsNewsList, ct);
        var techTask = ApiLoadHelper.SafeAsync(_news.GetTechNewsAsync, DemoDataProvider.TechNewsList, ct);
        var entrepreneurTask = ApiLoadHelper.SafeAsync(_news.GetEntrepreneurNewsAsync, DemoDataProvider.EntrepreneurNewsList, ct);
        var devTechTask = ApiLoadHelper.SafeAsync(_news.GetDevTechAsync, DemoDataProvider.DevTechList, ct);
        var weatherTask = ApiLoadHelper.SafeAsync(_curation.GetWeatherAsync, DemoDataProvider.DefaultWeather, ct);
        var f1TeaserTask = ApiLoadHelper.SafeAsync(_f1Brief.GetTeaserAsync, DemoDataProvider.F1TeaserFallback, ct);
        var f1BriefTask = ApiLoadHelper.SafeAsync(_f1Brief.GetNextRaceBriefAsync, DemoDataProvider.F1BriefFallback, ct);

        await Task.WhenAll(
            quotesTask, gainersTask, losersTask, marketNewsTask,
            politicsTask, techTask, entrepreneurTask, devTechTask,
            weatherTask, f1TeaserTask, f1BriefTask);

        var quotes = await quotesTask;
        var gainers = await gainersTask;
        var marketNews = await marketNewsTask;
        var politics = await politicsTask;
        var tech = await techTask;
        var weather = await weatherTask;
        var f1Teaser = await f1TeaserTask;
        var f1Brief = await f1BriefTask;

        return new DashboardViewModel
        {
            UserName = userName,
            Greeting = GreetingHelper.GetGreeting(userName),
            GeneratedAt = DateTime.Now,
            Weather = weather,
            QuickStatus = _curation.BuildQuickStatus(quotes, f1Teaser, tech),
            TodayImportant = _curation.BuildTodayImportant(marketNews, politics, tech, quotes, f1Teaser),
            F1Teaser = f1Teaser,
            TopEvents = _curation.BuildTopEvents(marketNews, politics, tech, f1Brief),
            StockOfTheDay = _curation.BuildStockOfTheDay(gainers, quotes),
            CompanyOfTheDay = _curation.GetCompanyOfTheDay(),
            DailyLearn = _curation.GetDailyLearnTerm(),
            QuickLinks = _curation.GetQuickLinks(),
            WeekOverview = _curation.BuildWeekOverview(f1Brief, weather),
            MarketQuotes = quotes,
            TopGainers = gainers,
            TopLosers = await losersTask,
            MarketNews = marketNews,
            PoliticsNews = politics,
            TechNews = tech,
            EntrepreneurNews = await entrepreneurTask,
            DevTech = await devTechTask,
            Watchlist = DemoDataProvider.DefaultWatchlist,
            GlossaryTerms = _glossary.GetAllTerms()
        };
    }

    public async Task<TodayImportantCard?> GetTodayImportantByIdAsync(string id, string userName = "Rion", CancellationToken ct = default)
    {
        var dashboard = await BuildDashboardAsync(userName, ct);
        return dashboard.TodayImportant.FirstOrDefault(e =>
            e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
