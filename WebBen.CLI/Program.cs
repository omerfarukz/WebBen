using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebBen.CLI.Common;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

var fileOption = new Option<FileInfo>(new[] {"-c", "--configuration-file"}, "The configuration file to use.");
var uriOption = new Option<Uri>(new[] {"-u", "--uri"}, "The URI to use.");

var rootCommand = new RootCommand
{
    fileOption, uriOption
};

rootCommand.SetHandler(async (FileInfo f) => { await ExecuteConfiguration(f.FullName); }, fileOption);
rootCommand.SetHandler(async (Uri u) =>
{
    Console.WriteLine($"Using URI: {u}");
    await ExecuteUri(u);
}, uriOption);

return rootCommand.Invoke(args);

async Task ExecuteUri(Uri uri)
{
    var configuration = new Configuration()
    {
        TestCases = new[]
        {
            new CaseConfiguration()
            {
                Name = "TestCase1",
                Uri = uri
            }
        }
    };
    
     var testContext = new HttpTestContext();
     var testCases = new List<TestCase>()
     {
         new TestCase(configuration.TestCases[0])
     };
     await testContext.Execute(testCases, null);
     await ExecuteTestCases(testCases, testContext, null);
}

async Task ExecuteConfiguration(string filePath)
{
    Console.WriteLine($"Reading configuration file: {filePath}");

    var configurationData = JsonNode.Parse(File.ReadAllText(filePath));
    var testConfigurations = configurationData?["TestCaseConfigurations"].Deserialize<CaseConfiguration[]>();
    var credentialConfigurations =
        configurationData?["CredentialConfigurations"].Deserialize<CredentialConfiguration[]>();

    if (testConfigurations == null)
        throw new InvalidDataException();

    Console.WriteLine("Configuration Loaded. Creating test cases");

    var testContext = new HttpTestContext();
    var testCases = testConfigurations
        .Select(f => new TestCase(f))
        .ToList();

    await ExecuteTestCases(testCases, testContext, credentialConfigurations);
}

async Task ExecuteTestCases(ICollection<TestCase> testCases, HttpTestContext testContext, ICollection<CredentialConfiguration>? credentialConfigurations)
{
    Console.WriteLine("Test cases created. Executing test cases");
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
}