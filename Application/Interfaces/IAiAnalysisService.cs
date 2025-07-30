namespace Application.Interfaces
{
    public interface IAiAnalysisService
    {
        IAsyncEnumerable<string> AnalyzeTextAsync(string text, CancellationToken cancellationToken = default);
    }
}
