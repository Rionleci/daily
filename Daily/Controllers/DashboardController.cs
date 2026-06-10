using Daily.DataLayer.Contracts;
using Daily.DataLayer.Services;
using Daily.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daily.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;
    private readonly IFormula1DataService _f1;
    private readonly IConfiguration _config;

    public DashboardController(
        IDashboardService dashboard,
        IFormula1DataService f1,
        IConfiguration config)
    {
        _dashboard = dashboard;
        _f1 = f1;
        _config = config;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userName = _config["Dashboard:UserName"] ?? "Rion";
        var model = await _dashboard.BuildDashboardAsync(userName, ct);
        return View(model);
    }

    [HttpGet("/dashboard/event/{id}")]
    public async Task<IActionResult> EventDetail(string id, CancellationToken ct)
    {
        var userName = _config["Dashboard:UserName"] ?? "Rion";
        var ev = await _dashboard.GetTodayImportantByIdAsync(id, userName, ct);
        if (ev is null) return NotFound();

        return View(new EventDetailViewModel
        {
            Event = ev,
            Greeting = GreetingHelper.GetGreeting(userName)
        });
    }

    [HttpGet("/dashboard/f1")]
    public async Task<IActionResult> F1Partial(CancellationToken ct)
    {
        try
        {
            var f1 = await _f1.GetCenterDataAsync(ct);
            if (f1 is null || !string.IsNullOrEmpty(f1.ErrorMessage))
                return Content("<section class='rd-section' id='f1'><p class='rd-section-sub'>F1-Daten vorübergehend nicht verfügbar.</p></section>");

            return PartialView("_Formula1Center", f1);
        }
        catch
        {
            return Content("<section class='rd-section' id='f1'><p class='rd-section-sub'>F1-Daten konnten nicht geladen werden.</p></section>");
        }
    }
}
