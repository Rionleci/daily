namespace Daily.Models;

public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string FinnhubApiKey { get; set; } = string.Empty;
    public string AlphaVantageApiKey { get; set; } = string.Empty;
    public string NewsApiKey { get; set; } = string.Empty;
    public string GNewsApiKey { get; set; } = string.Empty;
    public string ApiFootballKey { get; set; } = string.Empty;
    public string OpenWeatherApiKey { get; set; } = string.Empty;
    public string OpenAiApiKey { get; set; } = string.Empty;
    public int CacheDurationMinutes { get; set; } = 15;
}
