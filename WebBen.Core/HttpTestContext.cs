using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WebBen.Core.Configuration;
using WebBen.Core.CredentialProviders;
using WebBen.Core.Logging;

namespace WebBen.Core;

public class HttpTestContext
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

        _logger.Debug($"Resolution timer is '{Stopwatch.IsHighResolution}' precision");
    }

    /// <summary>
    /// TODO: return strong typed object
    /// </summary>
    /// <param name="testCase"></param>
    /// <param name="credentials"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task Execute(TestCase testCase, ICredentials? credentials)
    {
        if (testCase is null)
            throw new ArgumentNullException(nameof(testCase));

        _logger.Debug($"Executing test case: {testCase.Configuration.Name}");
        _logger.Debug($"Parallelism:\t\t{testCase.Configuration.Parallelism}");

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
        var actionBlock = new ActionBlock<TestCase>(async testCase =>
        {
            if (testCase == null)
                throw new ArgumentNullException(nameof(testCase));

            if (testCase.Configuration == null)
                throw new ArgumentNullException(nameof(testCase.Configuration));

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
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = parallelism
        });

        return actionBlock;
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
        if (testCaseInstance.Configuration.Cookies != null)
        {
            if (httpClientAccessor.CookieContainer == null)
                throw new InvalidProgramException("Cookie container is null");

            foreach (var cookie in testCaseInstance.Configuration.Cookies)
                httpClientAccessor.CookieContainer.Add(
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