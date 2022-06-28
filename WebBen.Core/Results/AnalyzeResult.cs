namespace WebBen.Core.Results;

public class AnalyzeResult
{
    public AnalyzeResult(IReadOnlyCollection<TestResult> results)
    {
        Results = results;
    }

    public IReadOnlyCollection<TestResult> Results { get; set; }
    public int MaxRequestsPerSecond { get; init; }
    public string[] Errors { get; set; }

    public void BuildSummaries()
    {
        foreach (var result in Results)
        {
            result.BuildSummaries();
        }
    }
}