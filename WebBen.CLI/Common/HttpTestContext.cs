using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Configuration;
using WebBen.CLI.CredentialProviders;
using WebBen.CLI.Extensions;

namespace WebBen.CLI.Common;

internal class HttpTestContext
{
    private readonly Dictionary<string, ICredentialProvider> _credentialProviders;
    private readonly ILogger _logger;

    public HttpTestContext(ILogger logger)
    {
        _logger = logger;
        _credentialProviders = new Dictionary<string, ICredentialProvider>
        {
            {nameof(NetworkCredentialProvider), new NetworkCredentialProvider()}
        };

        if (!Stopwatch.IsHighResolution)
            throw new InvalidOperationException("Low resolution timer(Stopwatch) is not supported.");
    }

    /// <summary>
    /// TODO: return strong typed object
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="credentials"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private async Task Execute(TestCase testCase, ICredentials? credentials)
    {
        if (testCase is null)
            throw new ArgumentNullException(nameof(testCase));

        _logger.Debug($"Executing test case: {testCase.Configuration.Name}");
        _logger.Debug($"Parallelism:\t\t{testCase.Configuration.Parallelism}");
        _logger.Debug($"BoundedCapacity:\t{testCase.Configuration.BoundedCapacity}");

        using var accessor = new WebBenHttpClientAccessor(testCase, credentials);
        var actionBlock = CreateActionBlock(
            accessor,
            testCase.Configuration.Parallelism,
            testCase.Configuration.BoundedCapacity
        );

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        for (var iterationCounter = 0;
             iterationCounter < testCase.Configuration.RequestCount;
             iterationCounter++)
        {
            await actionBlock.SendAsync(testCase);
        }

        _logger.Debug($"Waiting for all requests to finish...");
        
        actionBlock.Complete();
        await actionBlock.Completion;

        stopWatch.Stop();
        testCase.Elapsed = stopWatch.Elapsed;
    }

    public async Task<IEnumerable<TestCase>> Execute(IEnumerable<TestCase> testCases,
        ICollection<CredentialConfiguration>? credentialConfigurations)
    {
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));

        // Prevent multiple enumeration
        var enumerable = testCases as TestCase[] ?? testCases.ToArray();
        _logger.Debug($"Preparing test cases: {enumerable.Count()}");

        foreach (var testCase in enumerable)
        {
            ICredentials? credentials = null;
            if (testCase.Configuration.CredentialConfigurationKey != null)
            {
                if (credentialConfigurations == null)
                    throw new ArgumentNullException(nameof(credentialConfigurations));

                _logger.Debug($"Creating credentials for case '{testCase.Configuration.Name}");

                var credentialConfiguration = credentialConfigurations.SingleOrDefault(f =>
                    f.Key == testCase.Configuration.CredentialConfigurationKey);
                if (credentialConfigurations == null || credentialConfiguration == null)
                    throw new InvalidDataException(nameof(credentialConfiguration));

                if (!_credentialProviders.ContainsKey(credentialConfiguration.Provider))
                    throw new InvalidProgramException(
                        $"Credential provider '{credentialConfiguration.Provider}' is not registered."
                    );

                var provider = _credentialProviders[credentialConfiguration.Provider];
                if (credentialConfiguration.Data != null)
                    credentials = provider.FromConfiguration(credentialConfiguration.Data);
            }

            _logger.Debug($"Executing test case '{testCase.Configuration.Name}'");
            await Execute(testCase, credentials);
        }

        return enumerable;
    }

    public async Task<int> Execute(AnalyzeConfiguration configuration)
    {
        var lastMinimumRequestCount = 1;
        
        // Find best request per second by scaling time
        // 1, 2, 4, 8, 16 ... 2^32
        var requestCountCacheQueue = new Queue<int>(Enumerable.Range(0, 32).Select(f => (int) Math.Pow(2, f)));
        _logger.Info($"Range: {string.Join(',', requestCountCacheQueue)}");

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
                var results = await HttpTestContextExtensions.Execute(this, caseConfiguration);
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

        return lastMinimumRequestCount;
    }
    
    private ActionBlock<TestCase> CreateActionBlock(
        WebBenHttpClientAccessor webBenHttpClientAccessor,
        int parallelism, int boundedCapacity
    )
    {
        var actionBlock = new ActionBlock<TestCase>(async testCase =>
        {
            if (testCase == null)
                throw new ArgumentNullException(nameof(testCase));

            if (testCase.Configuration == null)
                throw new ArgumentNullException(nameof(testCase.Configuration));

            var httpRequestMessage = BuildHttpRequestMessage(testCase, webBenHttpClientAccessor);

            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var httpResponseMessage = await webBenHttpClientAccessor.Client.SendAsync(httpRequestMessage);
                if (testCase.Configuration!.FetchContent)
                    await httpResponseMessage.Content.ReadAsStringAsync();

                stopWatch.Stop();

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    testCase.Timings.Add(stopWatch.Elapsed);
                }
                else
                {
                    testCase.Errors.Add($"{httpResponseMessage.StatusCode}"); // TODO: encapsulate   
                    _logger.Error($"Test case {testCase.Configuration.Name} returned {httpResponseMessage.StatusCode}");
                }
            }
            catch (HttpRequestException e)
            {
                testCase.Errors.Add($"{e.StatusCode}: {e.Message}"); // TODO: encapsulate
                _logger.Error($"{testCase.Configuration.Name}: {e.Message}");
            }
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = boundedCapacity
        });

        return actionBlock;
    }

    private static HttpRequestMessage BuildHttpRequestMessage(
        TestCase testCaseInstance,
        WebBenHttpClientAccessor webBenHttpClientAccessor)
    {
        var httpRequestMessage = new HttpRequestMessage(
            new HttpMethod(testCaseInstance.Configuration.HttpMethod),
            testCaseInstance.Configuration.Uri
        );

        // Set cookies
        if (testCaseInstance.Configuration.Cookies != null)
        {
            if (webBenHttpClientAccessor.CookieContainer == null)
                throw new InvalidProgramException("Cookie container is null");

            foreach (var cookie in testCaseInstance.Configuration.Cookies)
                webBenHttpClientAccessor.CookieContainer.Add(
                    testCaseInstance.Configuration.Uri!,
                    new Cookie(cookie.Key, $"{cookie.Value}")
                );
        }

        // Set headers
        if (testCaseInstance.Configuration.Headers != null)
            foreach (var header in testCaseInstance.Configuration.Headers)
                httpRequestMessage.Headers.Add(header.Key, $"{header.Value}");

        // SetBody
        if (testCaseInstance.Configuration.Body != null)
        {
            var encoding = Encoding.GetEncoding(testCaseInstance.Configuration.Body.Encoding);
            httpRequestMessage.Content = new StringContent(
                testCaseInstance.Configuration.Body.Content,
                encoding,
                testCaseInstance.Configuration.Body.ContentType
            );
        }

        return httpRequestMessage;
    }
}