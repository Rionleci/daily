using Daily.DataLayer.ApiClients;

namespace Daily.DataLayer.Services;

public class F1ImageResolver
{
    private readonly OpenF1Client _openF1;

    public F1ImageResolver(OpenF1Client openF1) => _openF1 = openF1;

    public async Task<Dictionary<string, OpenF1Driver>> GetDriverLookupAsync(CancellationToken ct)
    {
        var drivers = await _openF1.GetLatestDriversAsync(ct);
        var lookup = new Dictionary<string, OpenF1Driver>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in drivers)
        {
            if (!string.IsNullOrEmpty(d.Acronym))
                lookup.TryAdd(d.Acronym, d);
            if (!string.IsNullOrEmpty(d.LastName))
                lookup.TryAdd(NormalizeName(d.LastName), d);
        }

        return lookup;
    }

    public string ResolveHeadshot(JolpicaDriver jolpica, Dictionary<string, OpenF1Driver> lookup)
    {
        if (!string.IsNullOrEmpty(jolpica.Code) && lookup.TryGetValue(jolpica.Code, out var byCode))
            return byCode.HeadshotUrl;

        var lastName = jolpica.Name.Split(' ').LastOrDefault() ?? "";
        if (lookup.TryGetValue(NormalizeName(lastName), out var byName))
            return byName.HeadshotUrl;

        // Teilstring-Match (z.B. driverId "antonelli" → Antonelli)
        var match = lookup.Values.FirstOrDefault(d =>
            NormalizeName(d.LastName) == NormalizeName(lastName) ||
            NormalizeName(jolpica.DriverId).Contains(NormalizeName(d.LastName)));

        return match?.HeadshotUrl ?? "";
    }

    public string ResolveTeamColour(JolpicaDriver jolpica, Dictionary<string, OpenF1Driver> lookup)
    {
        if (!string.IsNullOrEmpty(jolpica.Code) && lookup.TryGetValue(jolpica.Code, out var d))
            return $"#{d.TeamColour.TrimStart('#')}";

        return F1Catalog.GetTeam(jolpica.TeamId).Color;
    }

    private static string NormalizeName(string name) =>
        name.Replace(" ", "").Replace("-", "").ToLowerInvariant();
}
