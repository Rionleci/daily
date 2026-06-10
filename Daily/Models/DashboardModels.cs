namespace Daily.Models;

public record MarketQuote(string Symbol, string Name, decimal Price,
    decimal ChangeTodayPercent, decimal ChangeWeekPercent, IReadOnlyList<decimal> Sparkline);

public record MarketMover(string Symbol, string Name, decimal ChangePercent, decimal Price);

public record MarketNews(string Title, string Summary, string WhyImportant, string? SourceUrl = null);

public record PoliticsNews(string Title, string Summary, IReadOnlyList<string> EconomicImpact, string Region, string? SourceUrl = null);

public record TechNews(string Title, string Summary, string DeveloperRelevance,
    string EntrepreneurRelevance, string? Topic = null, string? SourceUrl = null);

public record BusinessNews(string Title, string WhatHappened, string WhyImportant,
    string Lesson, string Category, string? SourceUrl = null);

public record EntrepreneurNews(string Title, string Category, string Summary, string WhyImportant, string? SourceUrl = null);

public record DevTechItem(string Title, string Category, string Summary, string WhyForDevs, string? ProductPick, string? SourceUrl = null);

public record TodayHighlight(string Category, string Text, string Icon);

public record MorningReport(string Greeting, IReadOnlyList<string> FocusTopics, int RecommendedReadMinutes);

public record F1DriverStanding(int Position, string Name, string Team, int Points, int Wins, int Podiums, string Form);

public record F1TeamStanding(int Position, string Team, int Points, int Wins);

public record F1RaceResult(string RaceName, string Date, string Winner, string FastestLap, IReadOnlyList<string> Podium);

public record F1NextRace(string Name, string Circuit, string Country, DateTime RaceDate, long CountdownSeconds);

public record F1News(string Title, string Summary, string WhatItMeans);

public record FootballStandingRow(int Position, string Team, int Played, int Won, int Drawn,
    int Lost, int GoalsFor, int GoalsAgainst, int Points);

public record FootballMatch(string HomeTeam, string AwayTeam, string? Score, string Competition, string Date, bool IsHighlight);

public record FootballNews(string Title, string Summary, string Competition);

public record FootballLeague(string Name, string Icon,
    IReadOnlyList<FootballStandingRow> Standings,
    IReadOnlyList<FootballMatch> TopMatches,
    IReadOnlyList<FootballMatch> Results);

public record WatchlistItem(string Symbol, string CompanyName, decimal Price,
    decimal ChangePercent, string AnalystSentiment, IReadOnlyList<string> RecentNews);

public record GlossaryTerm(
    string Term,
    string Slug,
    string SimpleExplanation,
    string Category,
    string WhyImportant = "",
    string PracticalExample = "");

public record WeatherDay(string DayLabel, decimal HighC, decimal LowC, string Description, string Icon, int RainProbabilityPercent = 0);

public record WeatherSnapshot(
    decimal TemperatureC,
    string Description,
    IReadOnlyList<WeatherDay> Forecast,
    string Location = "Zürich",
    string Icon = "bi-sun-fill");

public record QuickStatusCard(string Emoji, string Label, string Value, string Subtext, bool? IsPositive = null);

public record TodayImportantCard(
    string Id,
    string Emoji,
    string Title,
    string Category,
    string Summary,
    string DetailBody,
    string? WhyImportant = null,
    string? SourceUrl = null);

public record F1StandingLine(int Position, string DriverName);

public record F1Teaser(
    string RaceName,
    int DaysLeft,
    long CountdownSeconds,
    IReadOnlyList<F1StandingLine> Standings,
    string CenterUrl);

public record F1Brief(string RaceName, string CircuitName, string Country, DateTime RaceDateTime, long CountdownSeconds, string? SourceUrl);

public record HeroInfoCard(string Icon, string Label, string Title, string Detail, string? SourceUrl = null);

public record HeadlineItem(string Emoji, string Category, string Title, string Summary, string? SourceUrl = null);

public record HeroNewspaper(
    string Greeting,
    IReadOnlyList<HeroInfoCard> InfoCards,
    IReadOnlyList<HeadlineItem> Headlines,
    int ReadMinutes);

public record DashboardEvent(int Rank, string Title, string Category, string Summary, string WhyImportant, string? SourceUrl = null);

public record CompanyOfTheDay(
    string Name,
    string Tagline,
    string WhatTheyDo,
    string WhyInteresting,
    string Industry,
    string? Valuation = null,
    string? SourceUrl = null);

public record StockRating(string Label, int Score, int MaxScore = 5);

public record StockOfTheDay(
    string Symbol,
    string Name,
    string WhatTheyDo,
    string WhyRelevant,
    string WhyInvestorsTalk,
    string Opportunities,
    string Risks,
    bool BeginnerFriendly,
    StockRating Risk,
    StockRating Growth,
    StockRating Stability,
    decimal Price,
    decimal ChangePercent,
    string? SourceUrl = null);

public record QuickLink(string Name, string Url, string Icon, string Color);

public record DailyLearnTerm(
    string Term,
    string Slug,
    string Explanation,
    string WhyImportant,
    string Example,
    int ReadSeconds);

public record WeekEvent(string DayLabel, string DateLabel, string Title, string Category, string Icon, string? SourceUrl = null);

public record WeekOverview(
    IReadOnlyList<WeekEvent> Events,
    string WeatherTrend,
    string F1Summary);
