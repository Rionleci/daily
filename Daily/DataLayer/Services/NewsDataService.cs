using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Daily.DataLayer.Services;

public class NewsDataService : INewsDataService
{
    private readonly NewsApiClient _newsApi;
    private readonly IMemoryCache _cache;

    public NewsDataService(NewsApiClient newsApi, IMemoryCache cache)
    {
        _newsApi = newsApi;
        _cache = cache;
    }

    public Task<IReadOnlyList<PoliticsNews>> GetPoliticsNewsAsync(CancellationToken ct = default) =>
        GetCachedAsync("news_politics", TimeSpan.FromMinutes(30),
            async token =>
            {
                if (_newsApi.IsConfigured)
                {
                    var api = await _newsApi.GetBusinessNewsAsync(token);
                    if (api.Count > 0) return api;
                }
                return DemoDataProvider.PoliticsNewsList;
            }, DemoDataProvider.PoliticsNewsList, ct);

    public Task<IReadOnlyList<TechNews>> GetTechNewsAsync(CancellationToken ct = default) =>
        GetCachedAsync("news_tech", TimeSpan.FromMinutes(30),
            async token =>
            {
                if (_newsApi.IsConfigured)
                {
                    var api = await _newsApi.GetTechNewsAsync(token);
                    if (api.Count > 0) return api;
                }
                return DemoDataProvider.TechNewsList;
            }, DemoDataProvider.TechNewsList, ct);

    public Task<IReadOnlyList<BusinessNews>> GetBusinessNewsAsync(CancellationToken ct = default) =>
        GetCachedAsync("news_business", TimeSpan.FromMinutes(30),
            _ => Task.FromResult(DemoDataProvider.BusinessNewsList),
            DemoDataProvider.BusinessNewsList, ct);

    public Task<IReadOnlyList<EntrepreneurNews>> GetEntrepreneurNewsAsync(CancellationToken ct = default) =>
        GetCachedAsync("news_entrepreneur", TimeSpan.FromMinutes(30),
            _ => Task.FromResult(DemoDataProvider.EntrepreneurNewsList),
            DemoDataProvider.EntrepreneurNewsList, ct);

    public Task<IReadOnlyList<DevTechItem>> GetDevTechAsync(CancellationToken ct = default) =>
        GetCachedAsync("news_devtech", TimeSpan.FromMinutes(30),
            _ => Task.FromResult(DemoDataProvider.DevTechList),
            DemoDataProvider.DevTechList, ct);

    public Task<IReadOnlyList<TodayHighlight>> GetTodayHighlightsAsync(CancellationToken ct = default) =>
        Task.FromResult(DemoDataProvider.TodayHighlightsList.Take(10).ToList() as IReadOnlyList<TodayHighlight>);

    public Task<MorningReport> GetMorningReportAsync(string userName, CancellationToken ct = default) =>
        Task.FromResult(DemoDataProvider.MorningReport(userName));

    private async Task<IReadOnlyList<T>> GetCachedAsync<T>(
        string key, TimeSpan duration,
        Func<CancellationToken, Task<IReadOnlyList<T>>> loader,
        IReadOnlyList<T> fallback,
        CancellationToken ct)
    {
        try
        {
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = duration;
                return await loader(ct);
            }) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
