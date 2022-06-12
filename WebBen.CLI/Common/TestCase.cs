using System.Collections.Concurrent;
using WebBen.CLI.Configuration;

namespace WebBen.CLI.Common;

public record TestCase(TestCaseConfiguration Configuration)
{
    public TestCaseConfiguration Configuration { get; } = Configuration ?? throw new ArgumentNullException(nameof(Configuration));

    /// <summary>
    /// Keep time spend in milliseconds
    /// </summary>
    public ConcurrentBag<TimeSpan> Timings { get; internal set; } = new();

    public ConcurrentBag<string> Errors { get; set; } = new();
}