using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

public class MarketDataService : IMarketDataService
{
    private readonly FinnhubClient _finnhub;
    private readonly CoinGeckoClient _coinGecko;
    private readonly IMemoryCache _cache;

    public MarketDataService(FinnhubClient finnhub, CoinGeckoClient coinGecko, IMemoryCache cache)
    {
        _finnhub = finnhub;
        _coinGecko = coinGecko;
        _cache = cache;
    }

    public async Task<IReadOnlyList<MarketQuote>> GetQuotesAsync(CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync("market_quotes", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            var quotes = new List<MarketQuote>(DemoDataProvider.MarketQuotes);

            if (_finnhub.IsConfigured)
            {
                var nvda = await _finnhub.GetQuoteAsync("NVDA", ct);
                if (nvda.HasValue)
                    quotes.Add(new MarketQuote("NVDA", "NVIDIA", nvda.Value, 0, 0,
                        [100m, 101m, 102m, 103m, 104m]));
            }

            var btcTask = _coinGecko.GetCryptoQuoteAsync("bitcoin", "Bitcoin", ct);
            var ethTask = _coinGecko.GetCryptoQuoteAsync("ethereum", "Ethereum", ct);
            await Task.WhenAll(btcTask, ethTask);

            var btc = await btcTask;
            if (btc is not null)
            {
                var idx = quotes.FindIndex(q => q.Symbol == "BTC");
                if (idx >= 0) quotes[idx] = btc;
                else quotes.Add(btc);
            }

            var eth = await ethTask;
            if (eth is not null)
            {
                var idx = quotes.FindIndex(q => q.Symbol == "ETH");
                if (idx >= 0) quotes[idx] = eth;
            }

            return (IReadOnlyList<MarketQuote>)quotes;
        }) ?? DemoDataProvider.MarketQuotes;
    }

    public Task<IReadOnlyList<MarketMover>> GetTopGainersAsync(CancellationToken ct = default) =>
        Task.FromResult(DemoDataProvider.TopGainers);

    public Task<IReadOnlyList<MarketMover>> GetTopLosersAsync(CancellationToken ct = default) =>
        Task.FromResult(DemoDataProvider.TopLosers);

    public async Task<IReadOnlyList<MarketNews>> GetMarketNewsAsync(CancellationToken ct = default)
    {
        try
        {
            return await _cache.GetOrCreateAsync("market_news", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                if (_finnhub.IsConfigured)
                {
                    var apiNews = await _finnhub.GetMarketNewsAsync(ct);
                    if (apiNews.Count > 0) return apiNews;
                }
                return DemoDataProvider.MarketNewsList;
            }) ?? DemoDataProvider.MarketNewsList;
        }
        catch
        {
            return DemoDataProvider.MarketNewsList;
        }
    }

    public async Task<WatchlistItem?> GetWatchlistQuoteAsync(string symbol, CancellationToken ct = default)
    {
        var demo = DemoDataProvider.WatchlistQuote(symbol.ToUpperInvariant(), symbol.ToUpperInvariant());

        if (_finnhub.IsConfigured)
        {
            var price = await _finnhub.GetQuoteAsync(symbol, ct);
            if (price.HasValue)
                return demo with { Price = price.Value };
        }

        return demo;
    }
}
