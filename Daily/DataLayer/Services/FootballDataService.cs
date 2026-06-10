using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;

namespace Daily.DataLayer.Services;

public class FootballDataService : IFootballDataService
{
    private readonly ApiFootballClient _apiFootball;

    public FootballDataService(ApiFootballClient apiFootball) => _apiFootball = apiFootball;

    public async Task<IReadOnlyList<FootballLeague>> GetLeaguesAsync(CancellationToken ct = default)
    {
        if (_apiFootball.IsConfigured)
        {
            var leagues = new List<FootballLeague>();
            var ids = new (int Id, string Name)[] {
                (2, "Champions League"), (39, "Premier League"),
                (78, "Bundesliga"), (207, "Schweizer Super League")
            };
            foreach (var (id, name) in ids)
            {
                var data = await _apiFootball.GetStandingsAsync(id, name, ct);
                leagues.AddRange(data);
            }
            if (leagues.Count > 0) return leagues;
        }
        return DemoDataProvider.FootballLeagues;
    }

    public Task<IReadOnlyList<FootballNews>> GetNewsAsync(CancellationToken ct = default) =>
        Task.FromResult(DemoDataProvider.FootballNewsList);
}
