using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebBen.Core.Configuration;
using WebBen.Core.Configuration.Source;
using WebBen.Core.Logging;

namespace WebBen.Core.Extensions;

public static class HttpTestContextExtensions
{
    public static async Task<IEnumerable<TestCase>> Execute(this HttpTestContext httpTestContext,
        CaseConfiguration caseConfiguration)
    {
        var configuration = new TestConfiguration
        {
            TestCaseConfigurations = new[] {caseConfiguration}
        };

        var testCases = new TestCase[]
        {
            new(configuration.TestCaseConfigurations[0])
        };

        return await httpTestContext.Execute(testCases, null);
    }

    public static async Task<IEnumerable<TestCase>> Execute(this HttpTestContext httpTestContext,
        IConfigurationSource configurationFile)
    {
        if (configurationFile == null)
            throw new ArgumentNullException(nameof(configurationFile));

        var configurationData = JsonNode.Parse(await configurationFile.GetContent());
        var testConfigurations = configurationData?["TestCaseConfigurations"].Deserialize<CaseConfiguration[]>();
        var credentialConfigurations =
            configurationData?["CredentialConfigurations"].Deserialize<CredentialConfiguration[]>();

        if (testConfigurations == null)
            throw new InvalidDataException();

        var testCases = testConfigurations
            .Select(f => new TestCase(f));

        return await httpTestContext.Execute(testCases, credentialConfigurations);
    }

    public static async Task<AnalyzeResult> Analyze(this HttpTestContext httpTestContext,
        AnalyzeConfiguration analyzeConfiguration, ILogger logger)
    {
        var maxRequestForSecond = 1;

        // Find best request per second by scaling time
        // 1, 2, 4, 8, 16 ... 2^32
        var requestCountCacheQueue = new Queue<int>(Enumerable.Range(0, 32).Select(f => (int) Math.Pow(2, f)));
        logger.Info($"Range: {string.Join(',', requestCountCacheQueue)}");

        var caseConfiguration = new CaseConfiguration
        {
            Name = "analyze",
            Uri = analyzeConfiguration.Uri,
            FetchContent = analyzeConfiguration.FetchContent,
            TimeoutInMs = analyzeConfiguration.TimeoutInMs,
            AllowRedirect = analyzeConfiguration.AllowRedirect
        };

        var resultBag = new ConcurrentBag<TestCase>();
        while (requestCountCacheQueue.Any())
        {
            var requestCount = requestCountCacheQueue.Dequeue();

            caseConfiguration.Parallelism = requestCount;
            caseConfiguration.RequestCount = requestCount;

            logger.Info($"Create request with maximum parallelism: {requestCount}");

            var failed = false;
            var trialTimespans = new TimeSpan[analyzeConfiguration.MaxTrialCount];
            for (var i = 0; i < analyzeConfiguration.MaxTrialCount; i++)
            {
                var context = new HttpTestContext(logger);

                var results = await context.Execute(caseConfiguration);
                var testCases = results as TestCase[] ?? results.ToArray();
                var result = testCases.First();

                if (!result.Errors.IsEmpty)
                {
                    logger.Info($"Error(s) occured {result.Errors.Count}");
                    failed = true;
                    break;
                }

                resultBag.Add(result);
                logger.Debug(testCases.AsTable());
                logger.Debug($"#{i + 1}. {result.Elapsed.TotalSeconds:N} sec(s)");
                trialTimespans[i] = result.Elapsed;
            }

            if (failed)
                break;

            var averageTiming = trialTimespans.Timing(analyzeConfiguration.CalculationFunction);
            logger.Info($"{analyzeConfiguration.CalculationFunction}: {averageTiming.TotalSeconds:N}");

            if (averageTiming.TotalSeconds < 1d)
            {
                maxRequestForSecond = requestCount;
            }
            else
            {
                logger.Debug($"requestCount:{requestCount}, lastMinimumRequestCount:{maxRequestForSecond}");

                // Found maximum request count. Now, try to determine limits.
                requestCountCacheQueue.Clear();
                Enumerable.Range(0, 31)
                    .Select(f => maxRequestForSecond + (int) Math.Pow(2, f))
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

                logger.Debug($"New range: [{string.Join(',', requestCountCacheQueue)}]");
            }
        }

        var analyzeResult = new AnalyzeResult(resultBag)
        {
            MaxRequestsPerSecond = maxRequestForSecond
        };
        return analyzeResult;
    }

    public static string AsTable(this IEnumerable<TestCase> testCases)
    {
        var asTable = testCases.ToStringTable(
            new[] {"Name", "Elapsed", "NoR", "Pll", "Err", "Avg", "P90", "Median"},
            f => f.Configuration.Name ?? string.Empty,
            f => f.Elapsed.TotalSeconds.ToString("N"),
            f => f.Configuration.RequestCount.ToString(),
            f => f.Configuration.Parallelism.ToString(),
            f => f.Errors.Count.ToString(),
            f => f.Timings.Average().TotalMilliseconds.ToString("N"),
            f => f.Timings.Percentile(0.9d).TotalMilliseconds.ToString("N"),
            f => f.Timings.Median().TotalMilliseconds.ToString("N")
        );

        return asTable;
    }
}