using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.CLI.Common;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

namespace WebBen.CLI.CommandLine;

internal class AnalyzeCommand : Command
{
    private readonly ILogger _logger;

    public AnalyzeCommand(ILogger logger) : base("analyze")
    {
        _logger = logger;

        AddAlias("analyse");
        AddArgument(new Argument<Uri>("uri", "The URI to use."));
        AddOption(new Option<bool>(new[] {"-f", "--fetch-content"}, "Whether to fetch the content of the URI."));
        AddOption(new Option<bool>(new[] {"-r", "--allow-redirect"}, "Whether to allow redirects."));
        AddOption(new Option<int>(new[] {"-t", "--timeout-in-ms"}, "The bounded capacity to use."));
        AddOption(new Option<int>(new[] {"-m", "--max-trial-count"}, "Iteration count for calculation. See -c"));
        AddOption(new Option<CalculationFunciton>(new[] {"-c", "--calculation-function"},
            "Function for RPS calculation"));

        Handler = CommandHandler.Create(Handle);
    }

    private async Task Handle(AnalyzeConfiguration configuration)
    {
        var lastMinimumRequestCount = 1;
        
        // Find best request per second by scaling time
        // 1, 2, 4, 8, 16 ... 2^32
        var requestCountCacheQueue = new Queue<int>(Enumerable.Range(0, 32).Select(f => (int) Math.Pow(2, f)));
        _logger.Info($"Range: {string.Join(',', requestCountCacheQueue)}");

        var context = new HttpTestContext(_logger);
        var caseConfiguration = new CaseConfiguration
        {
            Name = "analyze",
            Uri = configuration.Uri,
            FetchContent = configuration.FetchContent,
            TimeoutInMs = configuration.TimeoutInMs,
            AllowRedirect = configuration.AllowRedirect
        };

        while (requestCountCacheQueue.Any())
        {
            var requestCount = requestCountCacheQueue.Dequeue();

            caseConfiguration.Parallelism = requestCount;
            caseConfiguration.BoundedCapacity = requestCount;
            caseConfiguration.RequestCount = requestCount;
            
            _logger.Info($"Create request with maximum parallelism: {requestCount}");
            
            var failed = false;
            var trialTimespans = new TimeSpan[configuration.MaxTrialCount];
            for (var i = 0; i < configuration.MaxTrialCount; i++)
            {
                var results = await context.Execute(caseConfiguration);
                var testCases = results as TestCase[] ?? results.ToArray();
                var result = testCases.First();

                if (!result.Errors.IsEmpty)
                {
                    _logger.Info($"Error(s) occured {result.Errors.Count}");
                    failed = true;
                    break;
                }

                _logger.Debug(testCases.AsTable());
                _logger.Debug($"#{i+1}. {result.Elapsed.TotalSeconds:N} sec(s)");
                trialTimespans[i] = result.Elapsed;
            }

            if (failed)
                break;

            var averageTiming = trialTimespans.Timing(configuration.CalculationFunction);
            _logger.Info($"{configuration.CalculationFunction}: {averageTiming.TotalSeconds:N}");

            if (averageTiming.TotalSeconds < 1d)
            {
                lastMinimumRequestCount = requestCount;
            }
            else
            {
                _logger.Debug($"requestCount:{requestCount}, lastMinimumRequestCount:{lastMinimumRequestCount}");

                // Found maximum request count. Now, try to determine limits.
                requestCountCacheQueue.Clear();
                Enumerable.Range(0, 31)
                    .Select(f => lastMinimumRequestCount + (int) Math.Pow(2, f))
                    .Where(f => f < requestCount)
                    .ToList()
                    .ForEach(f => requestCountCacheQueue.Enqueue(f));

                if (requestCountCacheQueue.Any())
                {
                    var last = requestCountCacheQueue.Last();
                    var lastPartLength = requestCount - last;

                    // Fill last gap between last value of the limit and last value
                    Enumerable.Range(last, lastPartLength)
                        .ToList()
                        .ForEach(f => requestCountCacheQueue.Enqueue(f));
                }

                _logger.Debug($"New range: [{string.Join(',', requestCountCacheQueue)}]");
            }
        }

        _logger.Info($"Maximum request per second value is : {lastMinimumRequestCount}");
    }
}