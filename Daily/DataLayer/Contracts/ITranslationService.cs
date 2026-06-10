namespace Daily.DataLayer.Contracts;

public interface ITranslationService
{
    Task<string> TranslateEnToDeAsync(string text, CancellationToken ct = default);
}
