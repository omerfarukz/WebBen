using System.Text.Json;
using System.Text.Json.Nodes;
using WebBen.CLI.Common;
using WebBen.CLI.Configuration;

namespace WebBen.CLI.Extensions;

internal static class HttpTestContextExtensions
{
    public static async Task<IEnumerable<TestCase>> Execute(this HttpTestContext httpTestContext,
        CaseConfiguration caseConfiguration)
    {
        var configuration = new TestConfiguration
        {
            TestCases = new[] {caseConfiguration}
        };

        var testCases = new TestCase[]
        {
            new(configuration.TestCases[0])
        };
        
        return await httpTestContext.Execute(testCases, null);
    }

    public static async Task<IEnumerable<TestCase>> Execute(this HttpTestContext httpTestContext,
        FileInfo configurationFile)
    {
        if (configurationFile == null)
            throw new ArgumentNullException(nameof(configurationFile));
        if (!configurationFile.Exists)
            throw new FileNotFoundException(nameof(configurationFile));

        var configurationData = JsonNode.Parse(await File.ReadAllTextAsync(configurationFile.FullName));
        var testConfigurations = configurationData?["TestCaseConfigurations"].Deserialize<CaseConfiguration[]>();
        var credentialConfigurations =
            configurationData?["CredentialConfigurations"].Deserialize<CredentialConfiguration[]>();

        if (testConfigurations == null)
            throw new InvalidDataException();

        var testCases = testConfigurations
            .Select(f => new TestCase(f));

        return await httpTestContext.Execute(testCases, credentialConfigurations);
    }

    public static string AsTable(this IEnumerable<TestCase> testCases)
    {
        var asTable = testCases.ToStringTable(
            new[] {"Name", "NoR", "Pll", "BC", "Err", "Avg", "Min", "Max", "P90", "P80", "Median"},
            f => f.Configuration.Name ?? string.Empty,
            f => f.Configuration.RequestCount.ToString(),
            f => f.Configuration.Parallelism.ToString(),
            f => f.Configuration.BoundedCapacity.ToString(),
            f => f.Errors.Count.ToString(),
            f => f.Timings.Average().TotalMilliseconds.ToString("N"),
            f => f.Timings.Any() ? f.Timings.Min().TotalMilliseconds.ToString("N") : "0",
            f => f.Timings.Any() ? f.Timings.Max().TotalMilliseconds.ToString("N") : "0",
            f => f.Timings.Percentile(0.9d).TotalMilliseconds.ToString("N"),
            f => f.Timings.Percentile(0.8d).TotalMilliseconds.ToString("N"),
            f => f.Timings.Median().TotalMilliseconds.ToString("N")
        );

        return asTable;
    }
}