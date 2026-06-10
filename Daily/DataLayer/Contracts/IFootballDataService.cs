using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IFootballDataService
{
    Task<IReadOnlyList<FootballLeague>> GetLeaguesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FootballNews>> GetNewsAsync(CancellationToken ct = default);
}
