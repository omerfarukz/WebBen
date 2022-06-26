using WebBen.Core.Configuration;

namespace WebBen.Core.Results;

public record TestResultItem
{
    public TimeSpan[]? Timings { get; set; }
    public string[]? Errors { get; set; }
    public TimeSpan Elapsed { get; set; }
    public CaseConfiguration? Configuration { get; set; }
    public Dictionary<CalculationFunction, TimeSpan>? Calculations { get; set; }
}