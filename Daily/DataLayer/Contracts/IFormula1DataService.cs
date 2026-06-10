using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IFormula1DataService
{
    Task<Formula1CenterData> GetCenterDataAsync(CancellationToken ct = default);
}
