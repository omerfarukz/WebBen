using System.Text.Json;
using System.Text.Json.Nodes;
using WebBen.CLI.Common;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

var configurationFilePath = "Sample.json";
Console.WriteLine($"Reading configuration file: {configurationFilePath}");

var configObject = JsonNode.Parse(File.ReadAllText(configurationFilePath));
var testConfigurations = configObject?["TestCaseConfigurations"].Deserialize<List<TestCaseConfiguration>>();
var credentialConfigurations = configObject?["CredentialConfigurations"].Deserialize<List<CredentialConfiguration>>();

if (testConfigurations == null)
    throw new InvalidDataException();

Console.WriteLine("Configuration Loaded. Creating test cases");

var testContext = new HttpTestContext();
var testCases = testConfigurations
    .Select(f => new TestCase(f))
    .ToList();

Console.WriteLine($"Executing test cases: {testCases.Count}");
await testContext.Execute(testCases, credentialConfigurations);

var asTable = testCases.ToStringTable(
    new[] {"Name", "NoR", "Pll", "BC", "Err", "Avg", "Min", "Max", "P90", "P80", "Median"},
    f => f.Configuration.Name ?? string.Empty,
    f => f.Configuration.NumberOfRequests,
    f => f.Configuration.Parallelism,
    f => f.Configuration.BoundedCapacity,
    f => f.Errors.Count,
    f => f.Timings.Average().TotalMilliseconds.ToString("N"),
    f => f.Timings.Any() ? f.Timings.Min().TotalMilliseconds.ToString("N") : "0",
    f => f.Timings.Any() ? f.Timings.Max().TotalMilliseconds.ToString("N") : "0",
    f => f.Timings.Percentile(0.9d).TotalMilliseconds.ToString("N"),
    f => f.Timings.Percentile(0.8d).TotalMilliseconds.ToString("N"),
    f => f.Timings.Median().TotalMilliseconds.ToString("N")
);
Console.WriteLine();
Console.WriteLine(asTable);