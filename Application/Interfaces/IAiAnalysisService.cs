namespace Application.Interfaces
{
    public interface IAiAnalysisService
    {
        IAsyncEnumerable<string> AnalyzeTextAsync(string text, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> RephraseTextAsync(string text, string language, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> TranslateTextAsync(string text, string sourceLanguage, CancellationToken cancellationToken = default);
    }
}