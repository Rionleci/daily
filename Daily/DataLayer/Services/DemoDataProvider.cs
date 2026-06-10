using Daily.Models;

namespace Daily.DataLayer.Services;

public static class DemoDataProvider
{
    private static IReadOnlyList<decimal> Spark(bool up) =>
        up ? [100m, 101.2m, 100.8m, 102.1m, 103.2m, 104.1m] : [100m, 99.1m, 98.5m, 99.2m, 97.8m, 97.1m];

    public static IReadOnlyList<MarketQuote> MarketQuotes =>
    [
        new("^SSMI", "SMI", 11842.50m, 0.74m, 1.22m, Spark(true)),
        new("^GSPC", "S&P 500", 5287.40m, 0.44m, -0.35m, Spark(true)),
        new("^IXIC", "Nasdaq", 16742.80m, 0.68m, 1.49m, Spark(true)),
        new("^DJI", "Dow Jones", 38798.20m, -0.12m, -0.31m, Spark(false)),
        new("BTC", "Bitcoin", 67420.00m, 1.87m, 4.98m, Spark(true)),
        new("ETH", "Ethereum", 3521.40m, 1.98m, 4.23m, Spark(true)),
        new("XAU", "Gold", 2342.80m, 0.53m, 1.24m, Spark(true)),
        new("CL", "Öl (WTI)", 78.45m, -1.51m, -4.15m, Spark(false))
    ];

    public static IReadOnlyList<MarketMover> TopGainers =>
    [
        new("NVDA", "NVIDIA", 7.24m, 892.50m),
        new("SMCI", "Super Micro", 5.82m, 842.10m),
        new("ARM", "ARM Holdings", 4.91m, 128.40m)
    ];

    public static IReadOnlyList<MarketMover> TopLosers =>
    [
        new("TSLA", "Tesla", -3.42m, 178.20m),
        new("NKE", "Nike", -2.87m, 94.30m),
        new("BA", "Boeing", -2.15m, 178.90m)
    ];

    public static IReadOnlyList<MarketNews> MarketNewsList =>
    [
        new("NVIDIA steigt 7 %",
            "NVIDIA übertrifft die Quartalserwartungen dank starker Nachfrage nach AI-Chips.",
            "Steigende Nachfrage nach AI-Chips könnte weitere Technologie-Aktien positiv beeinflussen.",
            "https://www.reuters.com/technology/"),
        new("Fed-Sitzung am Mittwoch",
            "Der US-Notenbankrat tagt diese Woche. Märkte erwarten Hinweise zur Zinspolitik.",
            "Zinsentscheide beeinflussen direkt Aktien-, Anleihe- und Immobilienmärkte weltweit.",
            "https://www.federalreserve.gov/"),
        new("Ölpreise unter Druck",
            "Rohöl fällt aufgrund höherer Lagerbestände und schwächerer Nachfrageprognosen.",
            "Günstigeres Öl senkt Inflationsdruck, belastet aber Energieaktien.",
            "https://www.reuters.com/markets/commodities/")
    ];

    public static IReadOnlyList<PoliticsNews> PoliticsNewsList =>
    [
        new("SNB hält Leitzins bei 1.25 %",
            "Die Schweizerische Nationalbank lässt den Leitzins unverändert.",
            ["Hypothekarzinsen bleiben stabil", "Franken könnte weiter stark bleiben", "Immobilienmarkt profitiert"],
            "Schweiz", "https://www.snb.ch/de/mmr/reference/pre_20250320/source/pre_20250320"),
        new("EZB senkt Wachstumsprognose",
            "Die Europäische Zentralbank revidiert das Wirtschaftswachstum nach unten.",
            ["Exportorientierte Firmen betroffen", "Schwächerer Euro begünstigt Exporte", "Anleiherenditen unter Druck"],
            "Europa", "https://www.ecb.europa.eu/"),
        new("US-Arbeitsmarkt bleibt robust",
            "Die US-Arbeitslosenquote bleibt bei 3.8 %.",
            ["Fed könnte länger hohe Zinsen halten", "US-Konsum stützt globale Wirtschaft", "Tech profitiert"],
            "USA", "https://www.bls.gov/"),
        new("IMF warnt vor Handelskonflikten",
            "Der IWF senkt die globale Wachstumsprognose.",
            ["Lieferketten unter Druck", "Schwellenländer betroffen", "Diversifikation wichtiger"],
            "Weltwirtschaft", "https://www.imf.org/")
    ];

    public static IReadOnlyList<TechNews> TechNewsList =>
    [
        new("Microsoft stellt neue Copilot-Funktionen vor",
            "Microsoft erweitert Copilot in Office 365 mit tieferer Integration.",
            "Entwickler können über neue Graph-APIs auf Copilot-Funktionen zugreifen.",
            "Unternehmen können Produktivität steigern – Wettbewerbsvorteil für frühe Adopter.",
            "Microsoft", "https://www.microsoft.com/en-us/microsoft-365/blog/"),
        new("OpenAI kündigt GPT-5 Roadmap an",
            "OpenAI teilt erste Details zu GPT-5 mit: bessere Reasoning-Fähigkeiten.",
            "Neue API-Versionen erfordern Migration – modellunabhängige AI-Pipelines planen.",
            "AI-First-Startups können differenzieren.",
            "OpenAI", "https://openai.com/blog"),
        new("Apple Intelligence kommt in Europa",
            "Apple rollt AI-Funktionen schrittweise in der EU aus.",
            "iOS-Entwickler sollten App Intents evaluieren.",
            "Premium-Positionierung stärkt Ökosystem-Bindung.",
            "Apple", "https://www.apple.com/newsroom/"),
        new("Google Cloud wächst über 28 %",
            "Alphabet meldet starkes Wachstum im Cloud-Geschäft.",
            "GCP und Vertex AI werden wettbewerbsfähiger.",
            "Cloud-Migration bleibt Top-Priorität für CTOs.",
            "Google", "https://cloud.google.com/blog")
    ];

    public static IReadOnlyList<BusinessNews> BusinessNewsList =>
    [
        new("Stripe erhält Bewertung von 70 Mrd. USD",
            "Zahlungsdienstleister Stripe schliesst eine neue Finanzierungsrunde ab.",
            "Stripe bleibt eines der wertvollsten private Fintech-Unternehmen.",
            "Skalierbare Infrastruktur-Produkte können enorme Bewertungen erreichen.",
            "Finanzierung"),
        new("Databricks plant IPO",
            "Data-Analytics-Firma Databricks bereitet einen Börsengang vor.",
            "AI und Big Data bleiben heisse IPO-Themen.",
            "Timing und Profitabilitätsstory sind entscheidend für IPOs.",
            "IPO"),
        new("Salesforce übernimmt AI-Startup",
            "Salesforce kauft ein KI-Startup für 1.2 Mrd. USD.",
            "Konsolidierung im AI-Sektor beschleunigt sich.",
            "Strategische Positionierung kann Exit-Chancen erhöhen.",
            "Übernahme")
    ];

    public static IReadOnlyList<TodayHighlight> TodayHighlightsList =>
    [
        new("Börse", "NVIDIA steigt 7 % nach starken AI-Quartalszahlen", "bi-graph-up-arrow"),
        new("Wirtschaft", "SNB hält Leitzins bei 1.25 %", "bi-bank"),
        new("Politik", "EZB senkt Wachstumsprognose für die Eurozone", "bi-globe-europe-africa"),
        new("Tech", "Microsoft stellt neue Copilot-Funktionen vor", "bi-cpu"),
        new("F1", "McLaren bringt Upgrade nach Monaco", "bi-speedometer2"),
        new("Fussball", "Champions-League-Halbfinals: Real vs. Bayern", "bi-trophy"),
        new("Business", "Stripe bewertet mit 70 Mrd. USD", "bi-rocket-takeoff"),
        new("Krypto", "Bitcoin über 67'000 USD", "bi-currency-bitcoin")
    ];

    public static MorningReport MorningReport(string userName) => new(
        $"Guten Morgen {userName}.",
        [
            "NVIDIA steigt nach starken AI-Zahlen um über 7 %.",
            "Die SNB hält den Leitzins stabil bei 1.25 %.",
            "Das nächste Formel-1-Rennen in Monaco findet in wenigen Tagen statt.",
            "Microsoft stellt neue AI-Copilot-Funktionen für Office vor."
        ],
        8);

    public static F1NextRace NextRace()
    {
        var date = DateTime.UtcNow.Date.AddDays(4).AddHours(14);
        return new("Großer Preis von Monaco", "Circuit de Monaco", "Monaco", date,
            (long)(date - DateTime.UtcNow).TotalSeconds);
    }

    public static IReadOnlyList<F1DriverStanding> F1Drivers =>
    [
        new(1, "Max Verstappen", "Red Bull Racing", 248, 4, 6, "WWPDW"),
        new(2, "Lando Norris", "McLaren", 198, 2, 5, "WPWWL"),
        new(3, "Charles Leclerc", "Ferrari", 175, 1, 4, "PWWPD"),
        new(4, "Oscar Piastri", "McLaren", 162, 1, 3, "WPWWP"),
        new(5, "Lewis Hamilton", "Mercedes", 128, 0, 2, "PLWWP"),
        new(6, "George Russell", "Mercedes", 92, 0, 1, "LWPPL")
    ];

    public static IReadOnlyList<F1TeamStanding> F1Teams =>
    [
        new(1, "McLaren", 360, 3), new(2, "Ferrari", 315, 1),
        new(3, "Red Bull Racing", 310, 4), new(4, "Mercedes", 220, 0)
    ];

    public static IReadOnlyList<F1RaceResult> F1Results =>
    [
        new("GP Miami", "05.05.2026", "Lando Norris", "Max Verstappen", ["Norris", "Verstappen", "Leclerc"]),
        new("GP China", "21.04.2026", "Max Verstappen", "Lando Norris", ["Verstappen", "Norris", "Piastri"])
    ];

    public static IReadOnlyList<F1News> F1NewsList =>
    [
        new("McLaren bringt neues Upgrade nach Monaco",
            "McLaren setzt in Monaco auf ein neues Frontflügel-Paket.",
            "Das Upgrade könnte McLaren entscheidende Zehntelsekunden bringen."),
        new("Ferrari testet neue Bodenplatte",
            "Ferrari arbeitet an einer überarbeiteten Bodenplatte.",
            "Ferrari hofft, den Qualifying-Nachteil zu reduzieren.")
    ];

    public static IReadOnlyList<FootballLeague> FootballLeagues => [
        MakeLeague("Champions League", "bi-trophy",
            [("Real Madrid", 10, 8, 1, 1, 22, 8), ("Bayern München", 10, 7, 2, 1, 20, 9)],
            [("Real Madrid", "Bayern München", "21.05.", true)],
            [("Barcelona", "PSG", "4:1")]),
        MakeLeague("Premier League", "bi-flag",
            [("Arsenal", 36, 26, 6, 4, 78, 32), ("Liverpool", 36, 25, 7, 4, 82, 38)],
            [("Arsenal", "Manchester United", null, true)], []),
        MakeLeague("Bundesliga", "bi-flag-fill",
            [("Bayer Leverkusen", 32, 27, 4, 1, 82, 28), ("Bayern München", 32, 23, 5, 4, 78, 38)],
            [("Bayern München", "Dortmund", null, true)], []),
        MakeLeague("Schweizer Super League", "bi-flag",
            [("Young Boys", 32, 20, 6, 6, 58, 32), ("FC Basel", 32, 18, 8, 6, 52, 35)],
            [("Young Boys", "FC Basel", null, true)], [])
    ];

    public static IReadOnlyList<FootballNews> FootballNewsList =>
    [
        new("Champions-League-Halbfinals stehen fest", "Real Madrid trifft auf Bayern.", "Champions League"),
        new("Leverkusen vor Meistertitel", "Bayer Leverkusen kann die Meisterschaft perfekt machen.", "Bundesliga")
    ];

    public static IReadOnlyList<WatchlistItem> DefaultWatchlist =>
    [
        WatchlistQuote("MSFT", "Microsoft"), WatchlistQuote("AAPL", "Apple"),
        WatchlistQuote("NVDA", "NVIDIA"), WatchlistQuote("TSLA", "Tesla"),
        WatchlistQuote("AMZN", "Amazon"), WatchlistQuote("GOOGL", "Alphabet"),
        WatchlistQuote("META", "Meta")
    ];

    public static WatchlistItem WatchlistQuote(string symbol, string name) => symbol switch
    {
        "MSFT" => new(symbol, name, 415.20m, 1.24m, "Kaufen", ["Copilot treibt Cloud-Wachstum"]),
        "AAPL" => new(symbol, name, 189.50m, 0.82m, "Halten", ["Apple Intelligence in Europa"]),
        "NVDA" => new(symbol, name, 892.50m, 7.24m, "Kaufen", ["Quartalszahlen übertreffen Erwartungen"]),
        "TSLA" => new(symbol, name, 178.20m, -3.42m, "Halten", ["Lieferzahlen unter Erwartungen"]),
        "AMZN" => new(symbol, name, 182.40m, 0.95m, "Kaufen", ["AWS-Wachstum beschleunigt"]),
        "GOOGL" => new(symbol, name, 172.80m, 1.15m, "Kaufen", ["Cloud-Umsatz +28 %"]),
        "META" => new(symbol, name, 478.30m, 2.10m, "Kaufen", ["Reels-Monetarisierung wächst"]),
        _ => new(symbol, name, 100m, 0.5m, "Halten", [$"News zu {name}"])
    };

    public static IReadOnlyList<GlossaryTerm> Glossary => InvestGlossary;

    public static IReadOnlyList<GlossaryTerm> InvestGlossary =>
    [
        new("Aktie", "aktie",
            "Eine Aktie ist ein Anteil an einem Unternehmen. Wenn du eine Aktie kaufst, besitzt du einen kleinen Teil der Firma.",
            "Grundlagen", "Ohne Aktien kein Börsenhandel – das Fundament des Investierens.",
            "Du kaufst 1 Apple-Aktie für 190 CHF und profitierst, wenn Apple wächst."),
        new("ETF", "etf",
            "Ein ETF bildet einen Index nach – du kaufst viele Aktien auf einmal, diversifiziert und günstig.",
            "Finanzen", "Ideal für Anfänger: breite Streuung ohne Einzelaktien-Risiko.",
            "Ein SMI-ETF enthält die 20 grössten Schweizer Firmen in einem Produkt."),
        new("Dividende", "dividende",
            "Eine Dividende ist eine Gewinnausschüttung an Aktionäre – meist jährlich oder quartalsweise.",
            "Finanzen", "Passive Einkommensquelle neben Kursgewinnen.",
            "Nestlé zahlt z.B. regelmässig Dividenden an Aktionäre."),
        new("Inflation", "inflation",
            "Inflation bedeutet, dass Preise steigen. Dein Geld kauft dann weniger.",
            "Wirtschaft", "Beeinflusst Kaufkraft, Löhne und Zinsentscheide der SNB.",
            "Bei 3 % Inflation kostet dein Kaffee in 10 Jahren deutlich mehr."),
        new("Rezession", "rezession",
            "Eine Rezession ist eine Phase, in der die Wirtschaft schrumpft – oft zwei Quartale in Folge.",
            "Wirtschaft", "Aktien fallen oft, Arbeitslosigkeit steigt – wichtig für Risikomanagement.",
            "2008/2009: Globale Finanzkrise mit starkem Wirtschaftseinbruch."),
        new("Leitzins", "leitzins",
            "Der Leitzins ist der Zinssatz der Zentralbank. Er beeinflusst Kredite, Hypotheken und Sparen.",
            "Wirtschaft", "Steigende Zinsen belasten Immobilien und Konsum.",
            "SNB bei 1.25 %: Hypothekarzinsen bleiben moderat."),
        new("SMI", "smi",
            "Der Swiss Market Index (SMI) ist der wichtigste Schweizer Aktienindex mit 20 Blue-Chips.",
            "Märkte", "Barometer für die Schweizer Wirtschaft und dein Heimatmarkt.",
            "Nestlé, Roche und Novartis sind SMI-Schwergewichte."),
        new("Nasdaq", "nasdaq",
            "Der Nasdaq ist ein US-Technologieindex mit Fokus auf Tech und Growth-Aktien.",
            "Märkte", "Tech-Trends und AI-Boom spiegeln sich hier wider.",
            "NVIDIA, Apple und Microsoft prägen den Nasdaq."),
        new("KGV", "kgv",
            "Das Kurs-Gewinn-Verhältnis (KGV) zeigt, wie teuer eine Aktie im Verhältnis zum Gewinn ist.",
            "Bewertung", "Hilft bei der Einschätzung, ob eine Aktie über- oder unterbewertet ist.",
            "KGV 30 = Anleger zahlen 30× den Jahresgewinn pro Aktie."),
        new("Marktkapitalisierung", "marktkapitalisierung",
            "Marktkapitalisierung = Aktienkurs × Anzahl Aktien. Sie zeigt die Grösse eines Unternehmens.",
            "Bewertung", "Vergleich von Unternehmen unabhängig vom Aktienkurs.",
            "Apple ist mit über 3 Bio. USD eine der wertvollsten Firmen der Welt.")
    ];

    public static WeatherSnapshot DefaultWeather => new(
        22m, "Sonnig",
        [
            new("Heute", 28m, 16m, "Sonnig", "bi-sun-fill", 5),
            new("Mi", 26m, 15m, "Teilweise bewölkt", "bi-cloud-sun-fill", 15),
            new("Do", 24m, 14m, "Regenschauer", "bi-cloud-rain-fill", 65),
            new("Fr", 27m, 16m, "Sonnig", "bi-sun-fill", 10),
            new("Sa", 29m, 18m, "Heiss", "bi-sun-fill", 0)
        ],
        "Zürich", "bi-sun-fill");

    public static F1Teaser F1TeaserFallback => new(
        "Barcelona GP",
        4,
        4 * 86400L,
        [
            new(1, "Max Verstappen"),
            new(2, "Lando Norris"),
            new(3, "Charles Leclerc")
        ],
        "#f1");

    public static string RealEstateInsight =>
        "SNB hält Zinsen stabil – gute Zeit für Festhypotheken. Mietmarkt in Zürich bleibt angespannt.";

    public static StockOfTheDay StockOfTheDay => new(
        "NVDA", "NVIDIA",
        "Entwickelt Grafik- und AI-Chips für Rechenzentren, Gaming und autonomes Fahren.",
        "Profitiert massiv vom AI-Boom – Quartalszahlen übertrafen Erwartungen deutlich.",
        "Investoren sehen NVIDIA als zentralen Profiteur des AI-Infrastruktur-Booms.",
        "Dominante Position im AI-Chip-Markt. Cloud-Anbieter investieren Milliarden.",
        "Hohe Bewertung (KGV). Abhängigkeit vom AI-Zyklus und US-China-Exportrestriktionen.",
        true,
        new("Risiko", 4), new("Wachstum", 5), new("Stabilität", 3),
        892.50m, 7.24m, "https://investor.nvidia.com/");

    public static IReadOnlyList<CompanyOfTheDay> CompaniesOfTheDay =>
    [
        new("Stripe", "Zahlungsinfrastruktur für das Internet",
            "Stripe bietet APIs für Online-Zahlungen, Abos und Finanzprodukte.",
            "Mit 70 Mrd. USD Bewertung eines der wertvollsten private Fintechs – Vorbild für skalierbare B2B-Produkte.",
            "Fintech", "70 Mrd. USD", "https://stripe.com"),
        new("OpenAI", "Führend in generativer AI",
            "Entwickelt GPT-Modelle und AI-Tools wie ChatGPT für Unternehmen und Entwickler.",
            "Definiert den AI-Zyklus neu – jede Tech-Firma muss ihre AI-Strategie neu denken.",
            "AI", "100+ Mrd. USD (geschätzt)", "https://openai.com"),
        new("TSMC", "Weltgrösster Chip-Auftragsfertiger",
            "Fertigt Chips für Apple, NVIDIA und AMD in modernsten Fabs.",
            "Ohne TSMC kein AI-Boom – geopolitisch und wirtschaftlich kritisch.",
            "Halbleiter", "800+ Mrd. USD", "https://www.tsmc.com"),
        new("ASML", "Monopol bei EUV-Lithographie",
            "Liefert die Maschinen, mit denen die kleinsten Chip-Strukturen gefertigt werden.",
            "Schlüsseltechnologie für jeden Fortschritt in der Chip-Industrie.",
            "Halbleiter", "350+ Mrd. USD", "https://www.asml.com"),
        new("Palantir", "Datenanalyse für Regierung & Enterprise",
            "Verbindet grosse Datenmengen mit AI für Entscheidungsunterstützung.",
            "Profitiert vom Trend zu datengetriebenen Entscheidungen in Unternehmen und Behörden.",
            "Software", "60+ Mrd. USD", "https://www.palantir.com"),
        new("SpaceX", "Raumfahrt & Starlink",
            "Senkt Kosten für Raketenstarts und baut ein globales Satelliten-Internet auf.",
            "Neue Infrastruktur-Schicht – relevant für Kommunikation und Verteidigung.",
            "Raumfahrt", "350+ Mrd. USD (geschätzt)", "https://www.spacex.com")
    ];

    public static IReadOnlyList<EntrepreneurNews> EntrepreneurNewsList =>
    [
        new("Stripe schliesst Finanzierungsrunde bei 70 Mrd. USD",
            "Finanzierung", "Stripe bleibt eines der wertvollsten private Fintech-Unternehmen weltweit.",
            "Zeigt, dass Infrastruktur-Produkte mit Netzwerkeffekten enorme Bewertungen erreichen können.",
            "https://stripe.com/newsroom"),
        new("Salesforce übernimmt AI-Startup für 1.2 Mrd. USD",
            "Übernahme", "Konsolidierung im Enterprise-AI-Sektor beschleunigt sich.",
            "Grosse Player kaufen Innovation – relevant für Startup-Exits und Wettbewerb.",
            "https://www.salesforce.com/news/"),
        new("Databricks bereitet Börsengang vor",
            "IPO", "Data-Analytics-Firma mit starkem AI-Fokus plant IPO.",
            "AI und Big Data bleiben heisse IPO-Themen – Timing und Story entscheidend.",
            "https://www.databricks.com/blog"),
        new("OpenAI erweitert Enterprise-Angebot",
            "AI", "Neue API-Features und Sicherheitszertifizierungen für Grosskunden.",
            "Unternehmen können AI schneller produktiv einsetzen – Wettbewerbsvorteil für Early Adopter.",
            "https://openai.com/enterprise")
    ];

    public static IReadOnlyList<DevTechItem> DevTechList =>
    [
        new("Cursor 1.0 – AI-native IDE",
            "Entwicklung", "Cursor integriert AI direkt in den Editor-Workflow.",
            "Ideal für schnellere Refactorings und Code-Reviews mit AI-Unterstützung.",
            "Cursor", "https://cursor.com"),
        new(".NET 9 Performance-Updates",
            "Backend", "Microsoft verbessert JIT-Compiler und Native AOT weiter.",
            "Schnellere APIs und geringerer Memory-Footprint für Cloud-Deployments.",
            ".NET 9", "https://devblogs.microsoft.com/dotnet/"),
        new("React 19 stable",
            "Web", "Neue Compiler-Optimierungen und verbesserte Server Components.",
            "Weniger Boilerplate, bessere Performance für moderne Web-Apps.",
            "React", "https://react.dev/blog"),
        new("Vercel v0 & AI SDK",
            "Web", "Vercel erweitert AI-Tools für Full-Stack-Entwicklung.",
            "Schnelleres Prototyping von AI-Features im Frontend.",
            "Vercel", "https://vercel.com/blog")
    ];

    public static IReadOnlyList<HeadlineItem> HeroHeadlines =>
    [
        new("🔥", "Börse", "NVIDIA steigt nach starken AI-Zahlen",
            "Quartalszahlen übertrafen Erwartungen – AI-Nachfrage treibt den Sektor.",
            "https://www.reuters.com/technology/"),
        new("🏦", "Wirtschaft", "SNB hält Leitzins stabil",
            "Keine Änderung bei 1.25 % – Hypotheken und Franken bleiben im Fokus.",
            "https://www.snb.ch/"),
        new("🏎️", "Formel 1", "McLaren bringt Upgrade nach Barcelona",
            "Neues Aerodynamik-Paket soll im WM-Kampf entscheidend sein.",
            "https://www.formula1.com/"),
        new("🤖", "Tech", "Microsoft erweitert Copilot",
            "Tiefere Office-Integration – Produktivität für Unternehmen im Fokus.",
            "https://www.microsoft.com/en-us/microsoft-365/blog/"),
        new("🏠", "Immobilien", "Preise in Zürich steigen weiter",
            "Knappes Angebot und stabile Zinsen halten den Markt unter Druck.",
            "https://www.wohnungsboerse.net/")
    ];

    public static F1Brief F1BriefFallback => new(
        "Barcelona Grand Prix", "Circuit de Barcelona-Catalunya", "Spain",
        DateTime.UtcNow.AddDays(4), 4 * 86400L, "https://www.formula1.com/");

    public static IReadOnlyList<QuickLink> QuickLinks =>
    [
        new("ChatGPT", "https://chat.openai.com", "bi-chat-dots-fill", "#10a37f"),
        new("Cursor", "https://cursor.com", "bi-code-slash", "#6366f1"),
        new("GitHub", "https://github.com", "bi-github", "#24292f"),
        new("Outlook", "https://outlook.live.com", "bi-envelope-fill", "#0078d4"),
        new("Booking.com", "https://www.booking.com", "bi-calendar-check", "#003580"),
        new("Holiday Home", "https://www.airbnb.ch", "bi-house-heart-fill", "#ff5a5f"),
        new("Rinora Dashboard", "https://rinora.ch", "bi-grid-1x2-fill", "#7c3aed"),
        new("TradingView", "https://www.tradingview.com", "bi-graph-up", "#2962ff"),
        new("Datagrip", "https://www.jetbrains.com/datagrip/", "bi-database-fill", "#000000"),
        new("LightShark", "https://lightshark.io", "bi-lightning-fill", "#f59e0b")
    ];

    public static IReadOnlyList<WeekEvent> WeekEvents =>
    [
        new("Mittwoch", "11. Jun", "Apple WWDC Keynote", "Tech", "bi-apple", "https://developer.apple.com/wwdc/"),
        new("Donnerstag", "12. Jun", "SNB-Zinsentscheid", "Wirtschaft", "bi-bank", "https://www.snb.ch/"),
        new("Freitag", "13. Jun", "US-Inflationsdaten", "Märkte", "bi-bar-chart-fill", "https://www.bls.gov/")
    ];

    private static FootballLeague MakeLeague(string name, string icon,
        (string Team, int P, int W, int D, int L, int Gf, int Ga)[] table,
        (string Home, string Away, string? Date, bool Highlight)[] upcoming,
        (string Home, string Away, string Score)[] results)
    {
        var standings = table.Select((t, i) =>
            new FootballStandingRow(i + 1, t.Team, t.P, t.W, t.D, t.L, t.Gf, t.Ga, t.W * 3 + t.D)).ToList();
        var matches = upcoming.Select(m =>
            new FootballMatch(m.Home, m.Away, null, name, m.Date ?? "Demnächst", m.Highlight)).ToList();
        var resultMatches = results.Select(r =>
            new FootballMatch(r.Home, r.Away, r.Score, name, "Letzte Runde", false)).ToList();
        return new FootballLeague(name, icon, standings, matches, resultMatches);
    }
}
