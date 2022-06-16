using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection.Metadata;
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
        AddArgument(new Argument<Uri>("uri", "The URI to use."));

        Handler = CommandHandler.Create(Handle);
    }

    private async Task Handle(Uri uri)
    {
        // TODO: Move this logic to HttpTestContext
        
        // Find best request count by binary search
        // 1, 2, 4, 8, 16 ... 2^32
        int requestCount = 1;
        int lastMinimumRequestCount = 1;

        var requestCountCacheQueue = new Queue<int>(Enumerable.Range(0, 32).Select(f => (int) Math.Pow(2, f)));

        _logger.Info("Range: " + string.Join(',', requestCountCacheQueue));

        var context = new HttpTestContext(_logger);
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = uri;

        while (requestCountCacheQueue.Any())
        {
            requestCount = requestCountCacheQueue.Dequeue();

            caseConfiguration.Parallelism = requestCount;
            caseConfiguration.BoundedCapacity = requestCount;
            caseConfiguration.RequestCount = requestCount;

            _logger.Info($"Create request with maximum parallelism: {requestCount}");

            var results = await context.Execute(caseConfiguration);
            var testCases = results as TestCase[] ?? results.ToArray();
            var result = testCases.First();

            _logger.Debug(testCases.AsTable());

            if (result.Errors.Count > 0)
            {
                _logger.Info($"Error(s) occured {result.Errors.Count}");
                break;
            }

            _logger.Info(
                $"Total seconds: {result.Elapsed.TotalSeconds}, Request Count: {requestCount}, Errors: {result.Errors.Count}");

            if (result.Elapsed.TotalSeconds > 1)
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

                    _logger.Debug($"New range: {string.Join(',', requestCountCacheQueue)}");
                }
            }
            else
            {
                lastMinimumRequestCount = requestCount;
            }
        }

        _logger.Info($"Maximum request per second value is : {lastMinimumRequestCount}");
    }
}