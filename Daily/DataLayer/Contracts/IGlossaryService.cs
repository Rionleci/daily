using Daily.Models;

namespace Daily.DataLayer.Contracts;

public interface IGlossaryService
{
    IReadOnlyList<GlossaryTerm> GetAllTerms();
    GlossaryTerm? GetBySlug(string slug);
}
