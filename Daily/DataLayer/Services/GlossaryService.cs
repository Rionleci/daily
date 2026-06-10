using Daily.DataLayer.Contracts;
using Daily.Models;

namespace Daily.DataLayer.Services;

public class GlossaryService : IGlossaryService
{
    public IReadOnlyList<GlossaryTerm> GetAllTerms() => DemoDataProvider.InvestGlossary;

    public GlossaryTerm? GetBySlug(string slug) =>
        DemoDataProvider.InvestGlossary.FirstOrDefault(g =>
            g.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
}
