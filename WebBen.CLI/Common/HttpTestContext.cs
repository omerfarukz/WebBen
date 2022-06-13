using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks.Dataflow;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Configuration;
using WebBen.CLI.CredentialProviders;

namespace WebBen.CLI.Common;

internal class HttpTestContext
{
    private readonly Dictionary<string, ICredentialProvider> _credentialProviders;
    private readonly ILogger _logger;

    public HttpTestContext(ILogger logger)
    {
        _logger = logger;
        _credentialProviders = new Dictionary<string, ICredentialProvider>();
        _credentialProviders.Add(nameof(NetworkCredentialProvider), new NetworkCredentialProvider());

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
            await actionBlock.SendAsync(testCase);

        actionBlock.Complete();
        await actionBlock.Completion;
        stopWatch.Stop();
    }

    public async Task Execute(IEnumerable<TestCase> testCases,
        ICollection<CredentialConfiguration>? credentialConfigurations)
    {
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));

        foreach (var testCase in testCases)
        {
            ICredentials? credentials = null;
            if (testCase.Configuration.CredentialConfigurationKey != null)
            {
                if (credentialConfigurations == null)
                    throw new ArgumentNullException(nameof(credentialConfigurations));

                _logger.Log($"Creating credentials for '{testCase.Configuration.Name}");

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

            _logger.Log($"Executing test case '{testCase.Configuration.Name}'");
            await Execute(testCase, credentials);
        }
    }

    private ActionBlock<TestCase> CreateActionBlock(
        WebBenHttpClientAccessor webBenHttpClientAccessor,
        int parallelism, int boundedCapacity
    )
    {
        var actionBlock = new ActionBlock<TestCase>(async testCaseInstance =>
        {
            if (testCaseInstance == null)
                throw new ArgumentNullException(nameof(testCaseInstance));

            if (testCaseInstance.Configuration == null)
                throw new ArgumentNullException(nameof(testCaseInstance.Configuration));

            var httpRequestMessage = BuildHttpRequestMessage(testCaseInstance, webBenHttpClientAccessor);

            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var httpResponseMessage = await webBenHttpClientAccessor.Client.SendAsync(httpRequestMessage);
                if (testCaseInstance.Configuration!.FetchContent)
                    await httpResponseMessage.Content.ReadAsStringAsync();

                stopWatch.Stop();
                testCaseInstance.Timings.Add(stopWatch.Elapsed);

                if (!httpResponseMessage.IsSuccessStatusCode)
                    testCaseInstance.Errors.Add($"{httpResponseMessage.StatusCode}"); // TODO: encapsulate
            }
            catch (HttpRequestException e)
            {
                testCaseInstance.Errors.Add($"{e.StatusCode}: {e.Message}"); // TODO: encapsulate
            }
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = boundedCapacity
        });

        return actionBlock;
    }

    private HttpRequestMessage BuildHttpRequestMessage(TestCase testCaseInstance,
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