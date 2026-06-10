namespace Daily.DataLayer.Services;

public static class F1Catalog
{
    public static readonly IReadOnlyList<Models.F1BeginnerTerm> BeginnerTerms =
    [
        new("DRS", "drs", "DRS (Drag Reduction System) öffnet eine Klappe am Heckflügel auf geraden Strecken. Das reduziert Luftwiderstand und macht Überholen leichter."),
        new("Undercut", "undercut", "Beim Undercut fährst du früher in die Box, setzt frische Reifen und überhole deinen Gegner, weil du auf der Strecke schneller bist, während er noch auf abgenutzten Reifen fährt."),
        new("Overcut", "overcut", "Beim Overcut bleibst du länger draußen, fährst schnelle Runden und kommst nach dem Stopp vor dem Gegner raus – weil du auf der Strecke Zeit gutgemacht hast."),
        new("Pole Position", "pole-position", "Pole Position ist der erste Startplatz – gewonnen im Qualifying. Der Fahrer startet vor allen anderen und hat oft einen grossen Vorteil."),
        new("Parc Fermé", "parc-ferme", "Ab Parc Fermé dürfen Teams das Auto nur noch begrenzt ändern. Das verhindert, dass zwischen Qualifying und Rennen alles neu gebaut wird."),
        new("Ground Effect", "ground-effect", "Ground Effect nutzt Unterboden-Abtrieb: Das Auto wird durch Luft unter dem Boden stärker auf die Strecke gedrückt – mehr Grip in Kurven.")
    ];

    private static readonly Dictionary<string, TeamMeta> Teams = new()
    {
        ["mercedes"] = new("#00D2BE", "https://media.formula1.com/content/dam/fom-website/teams/2024/mercedes-logo.png"),
        ["ferrari"] = new("#DC0000", "https://media.formula1.com/content/dam/fom-website/teams/2024/ferrari-logo.png"),
        ["mclaren"] = new("#FF8000", "https://media.formula1.com/content/dam/fom-website/teams/2024/mclaren-logo.png"),
        ["red_bull"] = new("#3671C6", "https://media.formula1.com/content/dam/fom-website/teams/2024/red-bull-racing-logo.png"),
        ["aston_martin"] = new("#229971", "https://media.formula1.com/content/dam/fom-website/teams/2024/aston-martin-logo.png"),
        ["alpine"] = new("#0093CC", "https://media.formula1.com/content/dam/fom-website/teams/2024/alpine-logo.png"),
        ["williams"] = new("#64C4FF", "https://media.formula1.com/content/dam/fom-website/teams/2024/williams-logo.png"),
        ["rb"] = new("#6692FF", "https://media.formula1.com/content/dam/fom-website/teams/2024/rb-logo.png"),
        ["haas"] = new("#B6BABD", "https://media.formula1.com/content/dam/fom-website/teams/2024/haas-logo.png"),
        ["sauber"] = new("#52E252", "https://media.formula1.com/content/dam/fom-website/teams/2024/kick-sauber-logo.png"),
        ["audi"] = new("#F50537", "https://media.formula1.com/content/dam/fom-website/teams/2024/audi-logo.png"),
        ["cadillac"] = new("#909090", "https://media.formula1.com/content/dam/fom-website/teams/2024/cadillac-logo.png"),
    };

    private static readonly Dictionary<string, string> CountryFlags = new()
    {
        ["Spain"] = "🇪🇸", ["Monaco"] = "🇲🇨", ["Bahrain"] = "🇧🇭", ["Australia"] = "🇦🇺",
        ["USA"] = "🇺🇸", ["Italy"] = "🇮🇹", ["Canada"] = "🇨🇦", ["Austria"] = "🇦🇹",
        ["UK"] = "🇬🇧", ["Belgium"] = "🇧🇪", ["Japan"] = "🇯🇵", ["UAE"] = "🇦🇪",
        ["Netherlands"] = "🇳🇱", ["Hungary"] = "🇭🇺", ["Singapore"] = "🇸🇬", ["Mexico"] = "🇲🇽",
        ["Brazil"] = "🇧🇷", ["Qatar"] = "🇶🇦", ["Germany"] = "🇩🇪", ["France"] = "🇫🇷"
    };

    public static TeamMeta GetTeam(string teamId) =>
        Teams.GetValueOrDefault(teamId, new("#333333", ""));

    public static string GetFlag(string country) =>
        CountryFlags.GetValueOrDefault(country, "🏁");
}

public record TeamMeta(string Color, string LogoUrl);
