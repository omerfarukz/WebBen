using System.Collections.Concurrent;
using WebBen.CLI.Configuration;

namespace WebBen.CLI.Common;

public record TestCase
{
    public TestCaseConfiguration Configuration { get; }
    
    public TestCase(TestCaseConfiguration configuration)
    {
        this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Keep time spend in milliseconds
    /// </summary>
    public ConcurrentBag<TimeSpan> Timings { get; internal set; } = new();

    public ConcurrentBag<string> Errors { get; set; } = new();
}