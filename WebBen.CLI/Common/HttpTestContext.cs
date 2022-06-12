using System.Diagnostics;
using System.Net;
using System.Threading.Tasks.Dataflow;
using WebBen.CLI.Configuration;
using WebBen.CLI.CredentialProviders;

namespace WebBen.CLI.Common;

internal class HttpTestContext
{
    private const string DefaultCredentialProvider = nameof(NetworkCredentialProvider);

    private Dictionary<string, ICredentialProvider> _credentialProviders;

    public HttpTestContext()
    {
        _credentialProviders = new Dictionary<string, ICredentialProvider>();
        _credentialProviders.Add(nameof(NetworkCredentialProvider), new NetworkCredentialProvider());

        if (!Stopwatch.IsHighResolution)
            throw new InvalidOperationException("Low resolution timer(Stopwatch) is not supported.");
    }

    private async Task Execute(TestCase testCase, ICredentials? credentials)
    {
        if (testCase is null)
            throw new ArgumentNullException(nameof(testCase));

        using var accessor = new WebBenHttpClientAccessor(testCase, credentials);
        var actionBlock = CreateActionBlock(accessor, testCase.Configuration.Parallelism,
            testCase.Configuration.BoundedCapacity);

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        for (var iterationCounter = 0;
             iterationCounter < testCase.Configuration.NumberOfRequests;
             iterationCounter++)
        {
            await actionBlock.SendAsync(testCase);
        }

        actionBlock.Complete();
        await actionBlock.Completion;
        stopWatch.Stop();
    }

    public async Task Execute(IEnumerable<TestCase> testCases,
        IEnumerable<CredentialConfiguration>? credentialConfigurations)
    {
        if (testCases is null)
            throw new ArgumentNullException(nameof(testCases));

        foreach (var testCase in testCases)
        {
            ICredentials? credentials = null;
            if (testCase.Configuration.CredentialConfigurationKey != null)
            {
                Console.WriteLine($"Creating credentials for '{testCase.Configuration.Name}");

                var credentialConfiguration =
                    credentialConfigurations?.SingleOrDefault(f =>
                        f.Key == testCase.Configuration.CredentialConfigurationKey);
                if (credentialConfigurations == null || credentialConfiguration == null)
                    throw new InvalidDataException(nameof(credentialConfiguration));

                var provider = _credentialProviders[credentialConfiguration.Provider ?? DefaultCredentialProvider];
                credentials = provider.FromConfiguration(credentialConfiguration.Data);
            }

            Console.WriteLine($"Executing test case '{testCase.Configuration.Name}");
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

            var httpRequestMessage = new HttpRequestMessage(
                new HttpMethod(testCaseInstance.Configuration.HttpMethod),
                testCaseInstance.Configuration.Uri
            );

            // Set headers
            if (testCaseInstance.Configuration?.Headers != null)
            {
                foreach (var header in testCaseInstance.Configuration.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, $"{header.Value}");
                }
            }

            // SetBody
            if (!string.IsNullOrWhiteSpace(testCaseInstance.Configuration!.Body))
                httpRequestMessage.Content = new StringContent(testCaseInstance.Configuration.Body);

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
}