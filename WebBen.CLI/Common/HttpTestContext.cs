using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace WebBen.CLI.Common;

internal class HttpTestContext
{
    private async Task Execute(TestCase testCase)
    {
        if (!Stopwatch.IsHighResolution)
            throw new InvalidOperationException("Low resolution timer(Stopwatch) is not supported.");
        
        using var accessor = new WebBenHttpClientAccessor(
            testCase.Configuration.UseDefaultCredentials,
            testCase.Configuration.UseCookieContainer, null);
        
        var actionBlock = CreateActionBlock(
            accessor, testCase.Configuration.Parallelism, testCase.Configuration.BoundedCapacity);

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

    public async Task Execute(IEnumerable<TestCase> testCases)
    {
        foreach (var testCase in testCases)
            await Execute(testCase);
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
            
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod(testCaseInstance.Configuration.HttpMethod),
                testCaseInstance.Configuration.Uri);
            
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var httpResponseMessage = await webBenHttpClientAccessor.Client.SendAsync(httpRequestMessage);
                if (testCaseInstance.Configuration.FetchContent)
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