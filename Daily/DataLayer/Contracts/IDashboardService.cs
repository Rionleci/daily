using Daily.Models;
using Daily.ViewModels;

namespace Daily.DataLayer.Contracts;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildDashboardAsync(string userName = "Rion", CancellationToken ct = default);
    Task<TodayImportantCard?> GetTodayImportantByIdAsync(string id, string userName = "Rion", CancellationToken ct = default);
}
