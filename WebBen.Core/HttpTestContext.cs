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
    private readonly string _userAgent;
        
    public HttpTestContext(ILogger logger)
    {
        _logger = logger;
        _logger.Debug($"Timer resolution is set to high precision:'{Stopwatch.IsHighResolution}'");
        
        _userAgent = $"WebBen/{Assembly.GetExecutingAssembly().GetName().Version}";
        _credentialProviders = new Dictionary<string, ICredentialProvider>
        {
            {nameof(NetworkCredentialProvider), new NetworkCredentialProvider()}
        };
    }

    internal Task<TestResultItem> Execute(TestCase testCase, ICredentials? credentials)
    {
        if (testCase is null)
            throw new ArgumentNullException(nameof(testCase));

        return ExecuteInternal(testCase, credentials);
    }

    /// <summary>
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="credentials"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private async Task<TestResultItem> ExecuteInternal(TestCase testCase, ICredentials? credentials)
    {
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

    public Task<TestResult> Execute(
        IEnumerable<TestCase> testCases,
        IReadOnlyCollection<CredentialConfiguration>? credentialConfigurations
    )
    {
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));

        var testCasesAsArray = testCases.ToArray();
        foreach (var testCase in testCasesAsArray)
        {
            if (testCase == null)
                throw new ArgumentNullException(nameof(testCases));
            
            if (testCase.Configuration?.CredentialConfigurationKey == null)
                continue;
            if (credentialConfigurations == null)
                throw new ArgumentNullException(nameof(credentialConfigurations));
        }

        return ExecuteInternalAsync(testCasesAsArray, credentialConfigurations);
    }

    private async Task<TestResult> ExecuteInternalAsync(
            TestCase[] testCases,
            IReadOnlyCollection<CredentialConfiguration>? credentialConfigurations
        )
    {
        _logger.Debug($"Preparing test cases: {testCases.Length}");

        var testCaseIndex = 0;
        var testResult = new TestResult(new TestResultItem[testCases.Length]);

        foreach (var testCase in testCases)
        {
            ICredentials? credentials = null;
            if (testCase.Configuration.CredentialConfigurationKey != null)
            {
                _logger.Debug($"Creating credentials for case '{testCase.Configuration.Name}");

                var credentialConfiguration = credentialConfigurations!.Single(f =>
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

    internal Task CreateActionBlockInternal(HttpClientAccessor httpClientAccessor, TestCase testCase)
    {
        if (testCase == null)
            throw new ArgumentNullException(nameof(testCase));
        return CreateActionBlockInternalAsync(httpClientAccessor, testCase);
    }

    private async Task CreateActionBlockInternalAsync(HttpClientAccessor httpClientAccessor, TestCase testCase)
    {
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
                testCase.Errors.Add($"{httpResponseMessage.StatusCode}");   
                _logger.Error($"Test case {testCase.Configuration.Name} returned {httpResponseMessage.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            testCase.Errors.Add($"{e.StatusCode}: {e.Message}");
            _logger.Error($"{testCase.Configuration.Name}: {e.Message}");
        }
    }

    private HttpRequestMessage BuildHttpRequestMessage(
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
        httpRequestMessage.Headers.Add(userAgentKey, _userAgent);
        
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