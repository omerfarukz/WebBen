namespace WebBen.Core.Extensions;

public class AnalyzeResult
{
    public AnalyzeResult(IReadOnlyCollection<TestCase> results)
    {
        Results = results;
    }

    public IReadOnlyCollection<TestCase> Results { get; set; }
    public int MaxRequestsPerSecond { get; init; }
}