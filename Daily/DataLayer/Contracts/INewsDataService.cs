using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface INewsDataService
{
    Task<IReadOnlyList<PoliticsNews>> GetPoliticsNewsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TechNews>> GetTechNewsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BusinessNews>> GetBusinessNewsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EntrepreneurNews>> GetEntrepreneurNewsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DevTechItem>> GetDevTechAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TodayHighlight>> GetTodayHighlightsAsync(CancellationToken ct = default);
    Task<MorningReport> GetMorningReportAsync(string userName, CancellationToken ct = default);
}
