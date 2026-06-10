using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IF1BriefService
{
    Task<F1Brief> GetNextRaceBriefAsync(CancellationToken ct = default);
    Task<F1Teaser> GetTeaserAsync(CancellationToken ct = default);
}
