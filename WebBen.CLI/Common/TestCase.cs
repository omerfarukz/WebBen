using System.Collections.Concurrent;
using WebBen.CLI.Configuration;

namespace WebBen.CLI.Common;

internal record TestCase(CaseConfiguration Configuration)
{
    public CaseConfiguration Configuration { get; } =
        Configuration ?? throw new ArgumentNullException(nameof(Configuration));

    /// <summary>
    ///     Keep time spend in milliseconds
    /// </summary>
    public ConcurrentBag<TimeSpan> Timings { get; internal set; } = new();

    public ConcurrentBag<string> Errors { get; set; } = new();
}