namespace Daily.DataLayer.Services;

public static class GreetingHelper
{
    public static string GetGreeting(string userName, DateTime? at = null)
    {
        var hour = (at ?? DateTime.Now).Hour;
        var salutation = hour switch
        {
            >= 0 and < 12 => "Guten Morgen",
            >= 12 and < 18 => "Guten Nachmittag",
            _ => "Guten Abend"
        };
        return $"{salutation} {userName}.";
    }
}
