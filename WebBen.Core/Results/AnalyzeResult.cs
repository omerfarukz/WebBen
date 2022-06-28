namespace WebBen.Core.Results;

public record AnalyzeResult(IReadOnlyCollection<TestResult> Results, string[]? Errors)
{
    public int MaxRequestsPerSecond { get; init; }

    public void BuildSummaries()
    {
        foreach (var result in Results)
        {
            result.BuildSummaries();
        }
    }
}