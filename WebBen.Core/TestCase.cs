using System.Collections.Concurrent;
using WebBen.Core.Configuration;

namespace WebBen.Core;

public record TestCase(CaseConfiguration Configuration)
{
    public CaseConfiguration Configuration { get; } =
        Configuration ?? throw new ArgumentNullException(nameof(Configuration));

    public ConcurrentBag<TimeSpan> Timings { get; } = new();
    public ConcurrentBag<string> Errors { get; } = new(); 
}