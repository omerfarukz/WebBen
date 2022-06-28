using System.Text.Json;
using System.Text.Json.Nodes;
using WebBen.Core.Configuration;
using WebBen.Core.Configuration.Source;
using WebBen.Core.Logging;
using WebBen.Core.Results;

namespace WebBen.Core.Extensions;

public static class HttpTestContextExtensions
{
    public static async Task<TestResult> Execute(this HttpTestContext httpTestContext,
        CaseConfiguration caseConfiguration)
    {
        var testCases = new TestCase[]
        {
            new(caseConfiguration)
        };

        return await httpTestContext.Execute(testCases, null);
    }

    public static async Task<TestResult> Execute(this HttpTestContext httpTestContext,
        IConfigurationSource configurationSource)
    {
        if (configurationSource == null)
            throw new ArgumentNullException(nameof(configurationSource));

        var content = await configurationSource.GetContent();
        var configurationData = JsonNode.Parse(content)!;
        var testConfigurations = configurationData["TestCaseConfigurations"].Deserialize<CaseConfiguration[]>();
        var credentialConfigurations =
            configurationData["CredentialConfigurations"].Deserialize<CredentialConfiguration[]>();

        if (testConfigurations == null)
            throw new InvalidDataException();

        var testCases = testConfigurations.Select(configuration => new TestCase(configuration)).ToList();
        return await httpTestContext.Execute(testCases, credentialConfigurations);
    }

    public static async Task<AnalyzeResult> Analyze(this HttpTestContext httpTestContext,
        AnalyzeConfiguration analyzeConfiguration, ILogger logger)
    {
        var maxRequestForSecond = 1;

        // Find best request per second by scaling time
        // 1, 2, 4, 8, 16 ... 2^32
        var requestCountCacheQueue = new Queue<int>(Enumerable.Range(0, 32).Select(f => (int) Math.Pow(2, f)));
        logger.Debug($"Range: {string.Join(',', requestCountCacheQueue)}");

        var resultBag = new List<TestResult>();
        string[]? trialErrors = null;
        while (requestCountCacheQueue.Any())
        {
            var requestCount = requestCountCacheQueue.Dequeue();
            logger.Debug($"Create request with maximum parallelism: {requestCount}");

            var trialTimespans = new TimeSpan[analyzeConfiguration.MaxTrialCount];
            for (var i = 0; i < analyzeConfiguration.MaxTrialCount; i++)
            {
                var context = new HttpTestContext(logger);
                var caseConfiguration = new CaseConfiguration
                {
                    Uri = analyzeConfiguration.Uri,
                    FetchContent = analyzeConfiguration.FetchContent,
                    TimeoutInMs = analyzeConfiguration.TimeoutInMs,
                    AllowRedirect = analyzeConfiguration.AllowRedirect,
                    Parallelism = requestCount,
                    RequestCount = requestCount
                };
                var testResult = await context.Execute(caseConfiguration);
                resultBag.Add(testResult);

                var firstItem = testResult.Items.First();

                // If any trial failed, stop the analysis
                if (firstItem.Errors != null)
                {
                    logger.Debug($"Error(s) occured {firstItem.Errors.Length}");
                    trialErrors = firstItem.Errors;
                    break;
                }


                logger.Debug($"#{i + 1}. {firstItem.Elapsed.TotalSeconds:N} sec(s)");
                trialTimespans[i] = firstItem.Elapsed;
            }

            if (trialErrors != null)
                break;

            var averageTiming = trialTimespans.Calculate(analyzeConfiguration.CalculationFunction);
            logger.Debug($"{analyzeConfiguration.CalculationFunction}: {averageTiming.TotalSeconds:N}");

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
            MaxRequestsPerSecond = maxRequestForSecond,
            Errors= trialErrors
        };
        return analyzeResult;
    }
}