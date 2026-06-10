namespace Daily.DataLayer.Services;

public static class ApiLoadHelper
{
    public static async Task<T> SafeAsync<T>(
        Func<CancellationToken, Task<T>> load,
        T fallback,
        CancellationToken ct,
        int timeoutSeconds = 6)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
            return await load(timeout.Token);
        }
        catch
        {
            return fallback;
        }
    }
}
