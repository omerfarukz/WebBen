using System.Text.Json;
using WebBen.CLI.Common;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

var configuration =
    JsonSerializer.Deserialize<List<TestCaseConfiguration>>(
        File.ReadAllText("Sample.json")
    );

if (configuration == null)
    throw new InvalidDataException();

Console.WriteLine("Configuration Loaded. Executing tests...");

var testContext = new HttpTestContext();
var testCases = configuration
    .Select(f => new TestCase(f))
    .ToList();

await testContext.Execute(testCases);

var asTable = testCases.ToStringTable(
    new[] {"Name", "NoR", "Pll", "BC", "Err", "Avg", "Min", "Max", "P90", "P80", "Median"},
    f => f.Configuration.Name ?? string.Empty,
    f => f.Configuration.NumberOfRequests,
    f => f.Configuration.Parallelism,
    f => f.Configuration.BoundedCapacity,
    f => f.Errors.Count(),
    f => f.Timings.Average().TotalMilliseconds.ToString("N"),
    f => f.Timings.Min().TotalMilliseconds.ToString("N"),
    f => f.Timings.Max().TotalMilliseconds.ToString("N"),
    f => f.Timings.Percentile(0.9d).TotalMilliseconds.ToString("N"),
    f => f.Timings.Percentile(0.8d).TotalMilliseconds.ToString("N"),
    f => f.Timings.Median().TotalMilliseconds.ToString("N")
);
Console.WriteLine(asTable);