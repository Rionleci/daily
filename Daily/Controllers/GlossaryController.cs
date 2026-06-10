using Daily.DataLayer.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Daily.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GlossaryController : ControllerBase
{
    private readonly IGlossaryService _glossary;

    public GlossaryController(IGlossaryService glossary) => _glossary = glossary;

    [HttpGet("{slug}")]
    public IActionResult Get(string slug)
    {
        var term = _glossary.GetBySlug(slug);
        return term is null ? NotFound() : Ok(term);
    }
}
