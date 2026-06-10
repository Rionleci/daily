using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IMarketDataService
{
    Task<IReadOnlyList<MarketQuote>> GetQuotesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MarketMover>> GetTopGainersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MarketMover>> GetTopLosersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MarketNews>> GetMarketNewsAsync(CancellationToken ct = default);
    Task<WatchlistItem?> GetWatchlistQuoteAsync(string symbol, CancellationToken ct = default);
}
