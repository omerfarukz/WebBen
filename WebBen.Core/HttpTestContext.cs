using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WebBen.Core.Configuration;
using WebBen.Core.CredentialProviders;
using WebBen.Core.Logging;
using WebBen.Core.Results;
using System.Reflection;

namespace WebBen.Core;

public class HttpTestContext
{
    private readonly Dictionary<string, ICredentialProvider> _credentialProviders;
    private readonly ILogger _logger;
    private static readonly string DefaultUserAgent;

    static HttpTestContext()
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        DefaultUserAgent = $"WebBen/{assemblyVersion.Major}.{assemblyVersion.Minor}";
    }

    public HttpTestContext(ILogger logger)
    {
        _logger = logger;
        _logger.Debug($"Timer resolution is set to high precision:'{Stopwatch.IsHighResolution}'");
        
        _credentialProviders = new Dictionary<string, ICredentialProvider>
        {
            {nameof(NetworkCredentialProvider), new NetworkCredentialProvider()}
        };
    }

    /// <summary>
    ///     TODO: return strong typed object
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="credentials"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal async Task<TestResultItem> Execute(TestCase testCase, ICredentials? credentials)
    {
        if (testCase is null)
            throw new ArgumentNullException(nameof(testCase));

        _logger.Debug(
            $"Executing test case: {testCase.Configuration.Name}, Parallelism:\t\t{testCase.Configuration.Parallelism}");

        using var accessor = new HttpClientAccessor(testCase, credentials);
        var actionBlock = CreateActionBlock(
            accessor,
            testCase.Configuration.Parallelism
        );

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        for (var iterationCounter = 0;
             iterationCounter < testCase.Configuration.RequestCount;
             iterationCounter++)
        {
            await actionBlock.SendAsync(testCase);
        }

        _logger.Debug("Waiting for all requests to finish...");

        actionBlock.Complete();
        await actionBlock.Completion;

        stopWatch.Stop();

        var testResultItem = new TestResultItem
        {
            Elapsed = stopWatch.Elapsed,
            Errors = testCase.Errors.ToArray(),
            Timings = testCase.Timings.ToArray()
        };

        return testResultItem;
    }

    public async Task<TestResult> Execute(IReadOnlyCollection<TestCase> testCases,
        IReadOnlyCollection<CredentialConfiguration>? credentialConfigurations)
    {
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));

        // Prevent multiple enumeration
        var enumerableTestCases = testCases as TestCase[] ?? testCases.ToArray();
        _logger.Debug($"Preparing test cases: {enumerableTestCases.Length}");

        var testCaseIndex = 0;
        var testResult = new TestResult(new TestResultItem[enumerableTestCases.Length]);

        foreach (var testCase in enumerableTestCases)
        {
            ICredentials? credentials = null;
            if (testCase.Configuration.CredentialConfigurationKey != null)
            {
                if (credentialConfigurations == null)
                    throw new ArgumentNullException(nameof(credentialConfigurations));

                _logger.Debug($"Creating credentials for case '{testCase.Configuration.Name}");

                var credentialConfiguration = credentialConfigurations.Single(f =>
                    f.Key == testCase.Configuration.CredentialConfigurationKey);

                if (!_credentialProviders.ContainsKey(credentialConfiguration.Provider))
                    throw new InvalidProgramException(
                        $"Credential provider '{credentialConfiguration.Provider}' is not registered."
                    );

                var provider = _credentialProviders[credentialConfiguration.Provider];
                if (credentialConfiguration.Data != null)
                    credentials = provider.FromConfiguration(credentialConfiguration.Data);
            }

            _logger.Debug($"Executing test case '{testCase.Configuration.Name}'");
            testResult.Items[testCaseIndex] = await Execute(testCase, credentials);
            testResult.Items[testCaseIndex].Configuration = testCase.Configuration;

            testCaseIndex++;
        }

        return testResult;
    }

    public void AddCredentialProvider(ICredentialProvider credentialProvider)
    {
        if (credentialProvider == null)
            throw new ArgumentNullException(nameof(credentialProvider));

        _credentialProviders.Add(credentialProvider.GetType().Name, credentialProvider);
    }

    private ActionBlock<TestCase> CreateActionBlock(
        HttpClientAccessor httpClientAccessor,
        int parallelism
    )
    {
        var actionBlock = new ActionBlock<TestCase>(
            async testCase => { await CreateActionBlockInternal(httpClientAccessor, testCase); },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelism,
                BoundedCapacity = parallelism
            });

        return actionBlock;
    }

    internal async Task CreateActionBlockInternal(HttpClientAccessor httpClientAccessor, TestCase testCase)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));

        var httpRequestMessage = BuildHttpRequestMessage(testCase, httpClientAccessor);

        try
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var httpResponseMessage = await httpClientAccessor.Client.SendAsync(httpRequestMessage);
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
    }

    private static HttpRequestMessage BuildHttpRequestMessage(
        TestCase testCaseInstance,
        HttpClientAccessor httpClientAccessor)
    {
        var httpRequestMessage = new HttpRequestMessage(
            new HttpMethod(testCaseInstance.Configuration.HttpMethod),
            testCaseInstance.Configuration.Uri
        );

        // Set cookies
        foreach (var cookie in testCaseInstance.Configuration.Cookies)
        {
            httpClientAccessor.CookieContainer!.Add(
                testCaseInstance.Configuration.Uri!,
                new Cookie(cookie.Key, $"{cookie.Value}")
            );
        }

        const string userAgentKey = "User-Agent";
        httpRequestMessage.Headers.Add(userAgentKey, DefaultUserAgent);
        
        // Set headers
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