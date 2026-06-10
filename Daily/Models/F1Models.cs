namespace Daily.Models;

public record F1SeasonInfo(int Season, int CurrentRound, int TotalRounds, int RacesRemaining);

public record F1CalendarRace(
    int Round,
    string RaceName,
    string ShortName,
    string CircuitName,
    string Country,
    string? CountryFlagUrl,
    string? CircuitImageUrl,
    DateTime RaceDateTime,
    string DateDisplay,
    bool IsPast,
    string? Winner,
    string? PolePosition,
    string? FastestLap,
    IReadOnlyList<string>? Podium,
    IReadOnlyList<string>? Highlights,
    string CircuitId);

public record F1HeroRace(
    string RaceName,
    string CircuitName,
    string Country,
    string CountryCode,
    string FlagEmoji,
    string? CountryFlagUrl,
    DateTime RaceDateTime,
    long CountdownSeconds,
    decimal TrackLengthKm,
    int Laps,
    decimal TotalDistanceKm,
    string WeatherDescription,
    decimal TemperatureC,
    string CircuitLayoutUrl,
    string CircuitImageUrl,
    double Latitude,
    double Longitude);

public record F1TitleFightEntry(
    int Position,
    string DriverName,
    string Team,
    string TeamColor,
    int Points,
    int PointsGainedLast3,
    string Form,
    double BarPercent);

public record F1TitleFight(
    F1TitleFightEntry Leader,
    F1TitleFightEntry Challenger,
    int PointsGap,
    string MomentumDriver,
    string MomentumSummary);

public record F1TeamCard(
    int Position,
    string TeamId,
    string TeamName,
    string TeamColor,
    string LogoUrl,
    int Points,
    int Wins);

public record F1DriverCard(
    int Position,
    string DriverId,
    string DriverName,
    string DriverCode,
    string Team,
    string TeamColor,
    string PortraitUrl,
    int Points,
    int Wins,
    int Podiums,
    string Form,
    IReadOnlyList<string> LastResults);

public record F1RecentRaceCard(
    string RaceName,
    string ShortName,
    string Date,
    string Winner,
    string PolePosition,
    string FastestLap,
    IReadOnlyList<string> Podium,
    string CircuitId);

public record F1NewsStory(
    bool IsTopStory,
    string Title,
    string Summary,
    string WhyImportant,
    string ChampionshipImpact,
    string WhoBenefits,
    string ImpactLevel,
    string? SourceUrl = null);

public record F1RacePrediction(
    int Position,
    string DriverName,
    string Team,
    int ProbabilityPercent);

public record F1PredictionsSection(
    IReadOnlyList<F1RacePrediction> Predictions,
    string Reasoning);

public record F1BeginnerTerm(string Term, string Slug, string Explanation);

public record Formula1CenterData(
    F1SeasonInfo? SeasonInfo,
    IReadOnlyList<F1CalendarRace> UpcomingRaces,
    IReadOnlyList<F1CalendarRace> PastRaces,
    F1HeroRace? Hero,
    F1TitleFight? TitleFight,
    IReadOnlyList<F1TeamCard> Teams,
    IReadOnlyList<F1DriverCard> Drivers,
    IReadOnlyList<F1RecentRaceCard> RecentRaces,
    IReadOnlyList<F1NewsStory> News,
    F1PredictionsSection? Predictions,
    IReadOnlyList<F1BeginnerTerm> BeginnerTerms,
    DateTime FetchedAt,
    string? ErrorMessage = null);
