using System.Net.Http;
using Daily.DataLayer.ApiClients;
using Daily.DataLayer.Contracts;
using Daily.DataLayer.Services;
using Daily.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(AuthSettings.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
builder.Services.AddAuthorization();

static void ConfigureHttpClient(IHttpClientBuilder b) =>
    b.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false })
     .ConfigureHttpClient(client =>
     {
         client.DefaultRequestHeaders.UserAgent.ParseAdd(
             "RionDaily/1.0 (Business Intelligence Dashboard; contact@riondaily.local)");
         client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
     });

ConfigureHttpClient(builder.Services.AddHttpClient<FinnhubClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<CoinGeckoClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<NewsApiClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<JolpicaF1Client>());
ConfigureHttpClient(builder.Services.AddHttpClient<OpenF1Client>());
ConfigureHttpClient(builder.Services.AddHttpClient<OpenMeteoClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<F1NewsRssClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<WikipediaCircuitClient>());
ConfigureHttpClient(builder.Services.AddHttpClient<MultiviewerCircuitClient>());
builder.Services.AddHttpClient("F1Media", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "RionDaily/1.0 (Business Intelligence Dashboard; contact@riondaily.local)");
    client.DefaultRequestHeaders.Referrer = new Uri("https://www.formula1.com/");
}).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false });
ConfigureHttpClient(builder.Services.AddHttpClient<TranslationService>());
builder.Services.AddSingleton<F1ImageResolver>();
ConfigureHttpClient(builder.Services.AddHttpClient<ApiFootballClient>());

builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<INewsDataService, NewsDataService>();
builder.Services.AddScoped<IFormula1DataService, Formula1DataService>();
builder.Services.AddScoped<IFootballDataService, FootballDataService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<IF1BriefService, F1BriefService>();
builder.Services.AddScoped<IDashboardCurationService, DashboardCurationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IGlossaryService, GlossaryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
