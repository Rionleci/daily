namespace Daily.DataLayer.Services;

public static class F1MediaHelper
{
    private const string TrackBase =
        "https://media.formula1.com/content/dam/fom-website/2018-redesign-assets/Track%20icons%204x3/";

    private static readonly string BarcelonaFallback = TrackBase + "Spain%20carbon.png";

    public static string ResolveCircuitImage(string? url, string? circuitId = null, string? circuitName = null)
    {
        if (NeedsBarcelonaFallback(url, circuitId, circuitName))
            return BarcelonaFallback;
        return url ?? "";
    }

    public static string ProxyImage(string? url, string? circuitId = null, string? circuitName = null)
    {
        url = ResolveCircuitImage(url, circuitId, circuitName);
        return string.IsNullOrWhiteSpace(url) ? "" : $"/f1/image?url={Uri.EscapeDataString(url)}";
    }

    public static string? BarcelonaFallbackUrl => BarcelonaFallback;

    private static bool NeedsBarcelonaFallback(string? url, string? circuitId, string? circuitName) =>
        (url?.Contains("Barcelona-Catalunya", StringComparison.OrdinalIgnoreCase) ?? false) ||
        string.Equals(circuitId, "catalunya", StringComparison.OrdinalIgnoreCase) ||
        (circuitName?.Contains("Barcelona", StringComparison.OrdinalIgnoreCase) ?? false) ||
        (circuitName?.Contains("Catalunya", StringComparison.OrdinalIgnoreCase) ?? false);
}
