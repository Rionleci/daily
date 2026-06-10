using System.Text.Json;
using Daily.Models;

namespace Daily.DataLayer.ApiClients;

public class OpenMeteoClient
{
    private readonly HttpClient _http;

    public OpenMeteoClient(HttpClient http) => _http = http;

    public async Task<(decimal TemperatureC, string Description)> GetCurrentWeatherAsync(
        double latitude, double longitude, CancellationToken ct = default)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}" +
                  "&current=temperature_2m,weather_code&timezone=auto";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var current = doc.RootElement.GetProperty("current");
        var temp = current.GetProperty("temperature_2m").GetDecimal();
        var code = current.GetProperty("weather_code").GetInt32();

        return (temp, MapWeatherCode(code));
    }

    private static string MapWeatherCode(int code) => code switch
    {
        0 => "Klar",
        1 or 2 or 3 => "Teilweise bewölkt",
        45 or 48 => "Nebel",
        51 or 53 or 55 => "Nieselregen",
        61 or 63 or 65 => "Regen",
        71 or 73 or 75 => "Schnee",
        80 or 81 or 82 => "Regenschauer",
        95 or 96 or 99 => "Gewitter",
        _ => "Wechselhaft"
    };

    public async Task<WeatherSnapshot> GetWeatherSnapshotAsync(
        double latitude = 47.3769,
        double longitude = 8.5417,
        CancellationToken ct = default)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}" +
                  "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                  "&forecast_days=5&timezone=auto&current=temperature_2m,weather_code";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = doc.RootElement;
        var current = root.GetProperty("current");
        var temp = current.GetProperty("temperature_2m").GetDecimal();
        var currentCode = current.GetProperty("weather_code").GetInt32();

        var daily = root.GetProperty("daily");
        var times = daily.GetProperty("time").EnumerateArray().ToList();
        var maxTemps = daily.GetProperty("temperature_2m_max").EnumerateArray().ToList();
        var minTemps = daily.GetProperty("temperature_2m_min").EnumerateArray().ToList();
        var codes = daily.GetProperty("weather_code").EnumerateArray().ToList();
        var rainProbs = daily.GetProperty("precipitation_probability_max").EnumerateArray().ToList();

        var culture = new System.Globalization.CultureInfo("de-CH");
        var forecast = new List<WeatherDay>();
        for (var i = 0; i < Math.Min(5, times.Count); i++)
        {
            var date = DateTime.Parse(times[i].GetString()!);
            var label = i == 0 ? "Heute" : date.ToString("ddd", culture);
            var code = codes[i].GetInt32();
            forecast.Add(new WeatherDay(
                label,
                Math.Round(maxTemps[i].GetDecimal()),
                Math.Round(minTemps[i].GetDecimal()),
                MapWeatherCode(code),
                MapWeatherIcon(code),
                rainProbs[i].GetInt32()));
        }

        return new WeatherSnapshot(
            temp,
            MapWeatherCode(currentCode),
            forecast,
            "Zürich",
            MapWeatherIcon(currentCode));
    }

    private static string MapWeatherIcon(int code) => code switch
    {
        0 => "bi-sun-fill",
        1 or 2 or 3 => "bi-cloud-sun-fill",
        45 or 48 => "bi-cloud-fog2-fill",
        51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82 => "bi-cloud-rain-fill",
        71 or 73 or 75 => "bi-snow",
        95 or 96 or 99 => "bi-cloud-lightning-rain-fill",
        _ => "bi-cloud-fill"
    };
}
