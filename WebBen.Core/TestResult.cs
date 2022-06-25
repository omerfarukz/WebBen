using System.Collections.Concurrent;

namespace WebBen.Core;

public record TestResult
{
    
}

public record TestResultItem
{
    public ConcurrentBag<TimeSpan> Timings { get; } = new();
    public ConcurrentBag<string> Errors { get; } = new();
    public TimeSpan Elapsed { get; internal set; }
}