using System.Net.Http.Headers;
using System.Xml.Linq;
using Daily.Models;

namespace Daily.DataLayer.ApiClients;

public class F1NewsRssClient
{
    private readonly HttpClient _http;

    private static readonly (string Url, string Name)[] Feeds =
    [
        ("https://www.motorsport.com/rss/f1/news/", "Motorsport.com"),
        ("https://www.autosport.com/rss/feed/f1", "Autosport")
    ];

    public F1NewsRssClient(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RionDaily", "1.0"));
    }

    public async Task<IReadOnlyList<(string Title, string Summary, string Url)>> FetchNewsAsync(
        int count = 6, CancellationToken ct = default)
    {
        var items = new List<(string Title, string Summary, string Url)>();

        foreach (var (url, _) in Feeds)
        {
            try
            {
                var xml = await _http.GetStringAsync(url, ct);
                var doc = XDocument.Parse(xml);
                var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

                foreach (var item in doc.Descendants(ns + "item").Take(count))
                {
                    var title = StripHtml(item.Element(ns + "title")?.Value ?? "");
                    var summary = StripHtml(item.Element(ns + "description")?.Value ?? "");
                    var link = item.Element(ns + "link")?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(title))
                        items.Add((title, Truncate(summary, 300), link));
                }
            }
            catch { /* try next feed */ }

            if (items.Count >= count) break;
        }

        return items.DistinctBy(i => i.Title).Take(count).ToList();
    }

    public IReadOnlyList<F1NewsStory> EnrichNews(IReadOnlyList<(string Title, string Summary, string Url)> raw)
    {
        return raw.Select((item, index) => EnrichSingle(item.Title, item.Summary, item.Url, index == 0)).ToList();
    }

    private static F1NewsStory EnrichSingle(string title, string summary, string url, bool isTop)
    {
        var lower = (title + " " + summary).ToLowerInvariant();

        var whyImportant = DetectWhyImportant(lower, title);
        var championshipImpact = DetectChampionshipImpact(lower);
        var whoBenefits = DetectWhoBenefits(lower, title);
        var impactLevel = DetectImpactLevel(lower);

        return new F1NewsStory(isTop, title, summary, whyImportant, championshipImpact, whoBenefits, impactLevel, url);
    }

    private static string DetectWhyImportant(string lower, string title)
    {
        if (lower.Contains("upgrade") || lower.Contains("update") || lower.Contains("package"))
            return "Ein technisches Update kann die Leistung des Autos deutlich verändern – besonders relevant für Qualifying und Rennpace.";
        if (lower.Contains("penalty") || lower.Contains("strafe") || lower.Contains("grid"))
            return "Strafen und Startplatz-Änderungen beeinflussen direkt die WM-Situation und die Renntaktik.";
        if (lower.Contains("test") || lower.Contains("fp1") || lower.Contains("practice"))
            return "Testfahrten und Freie Trainings geben erste Hinweise auf die Wettbewerbsfähigkeit vor dem Rennen.";
        if (lower.Contains("contract") || lower.Contains("sign") || lower.Contains("deal"))
            return "Fahrer- oder Team-Entscheidungen können die langfristige WM-Balance verschieben.";
        return $"Diese Entwicklung rund um «{Truncate(title, 60)}» könnte das Kräfteverhältnis im aktuellen Formel-1-Feld beeinflussen.";
    }

    private static string DetectChampionshipImpact(string lower)
    {
        if (lower.Contains("upgrade") || lower.Contains("championship") || lower.Contains("title"))
            return "Wenn die Änderung funktioniert, könnte der Titelkampf enger werden und die Punkteverteilung der nächsten Rennen verändern.";
        if (lower.Contains("penalty"))
            return "Direkte Auswirkung auf Startposition und Punktepotenzial im kommenden Grand Prix.";
        return "Mittelfristige Auswirkung auf Form und Konstanz der betroffenen Teams im WM-Kampf.";
    }

    private static string DetectWhoBenefits(string lower, string title)
    {
        var teams = new[] { "mercedes", "ferrari", "mclaren", "red bull", "williams", "alpine", "aston", "haas", "rb", "sauber" };
        var found = teams.Where(t => lower.Contains(t)).ToList();
        if (found.Count > 0)
            return string.Join(", ", found.Select(t => char.ToUpper(t[0]) + t[1..])) + " – sowie deren Fahrer.";
        return "Das betroffene Team und seine Fahrer profitieren am meisten, falls die Entwicklung wie geplant greift.";
    }

    private static string DetectImpactLevel(string lower)
    {
        if (lower.Contains("upgrade") || lower.Contains("engine") || lower.Contains("championship"))
            return "Mittel bis hoch";
        if (lower.Contains("fp1") || lower.Contains("test driver") || lower.Contains("reserve"))
            return "Niedrig";
        return "Mittel";
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ").Trim();

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max].TrimEnd() + "…";
}
