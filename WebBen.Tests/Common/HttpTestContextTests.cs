using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.Configuration.Source;
using WebBen.Core.Extensions;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class HttpTestContextTests
{
    private HttpTestContext _httpTestContext;
    private WebApplication _server;
    private Uri _uri = null!;

    [SetUp]
    public void Setup()
    {
        _server = MockWebApplication.CreateServer();
        _uri = new Uri(_server.Urls.First());
        _httpTestContext = new HttpTestContext(MockLogger.Instance);
        _httpTestContext.AddCredentialProvider(new MockCredentialProvider());
    }

    [Test]
    public void Add_Null_Credential_Provider_Should_Throw()
    {
        _httpTestContext.AddCredentialProvider(new MockCredentialProvider2());
        Assert.Throws<ArgumentNullException>(() => _httpTestContext.AddCredentialProvider(null!));
    }

    [Test]
    public async Task Pass_Null_To_CreateActionBlockInternal_Should_Throw()
    {
        var testCase = new TestCase(new CaseConfiguration
        {
            Uri = _uri,
            FetchContent = true
        });

        var httpClientAccessor = new HttpClientAccessor(testCase, null);
        await _httpTestContext.CreateActionBlockInternal(httpClientAccessor, testCase);

        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _httpTestContext.CreateActionBlockInternal(httpClientAccessor, null!));
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _httpTestContext.CreateActionBlockInternal(httpClientAccessor, new TestCase(null!)));

        testCase = new TestCase(new CaseConfiguration
        {
            Uri = new Uri($"{_uri}/notfound"),
            FetchContent = true
        });
        await _httpTestContext.CreateActionBlockInternal(httpClientAccessor, testCase);

        testCase = new TestCase(new CaseConfiguration
        {
            Uri = new Uri("http://foo.bar.local"),
            FetchContent = false
        });

        await _httpTestContext.CreateActionBlockInternal(httpClientAccessor, testCase);
    }

    [Test]
    public void Http_Request_Exception_Should_Catch()
    {
        var configuration = new CaseConfiguration();
        configuration.Uri = new Uri("http://foo.bar.local");
        configuration.RequestCount = 1;

        var testCases = _httpTestContext.Execute(new List<TestCase>
        {
            new(configuration)
        }, null).Result;

        Assert.IsNotEmpty(testCases.Items);
        Assert.AreEqual(testCases.Items.Length, 1);
        Assert.IsNotEmpty(testCases.Items[0].Errors);
    }

    [Test]
    public void Add_Null_Configuration_Args_Should_Throw()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var testCases = await _httpTestContext.Execute(new[]
            {
                new TestCase(null!)
            }, null);
        });
    }

    [Test]
    public void Add_Null_Test_Case_In_Collection_Args_Should_Throw()
    {
        Assert.Throws<AggregateException>(() =>
        {
            var testCases = _httpTestContext.Execute(new TestCase[]
            {
                null
            }, null).Result;
        });
    }

    [Test]
    public void Add_Non_Existing_Credential_Key_Should_Throw()
    {
        Assert.Throws<AggregateException>(() =>
        {
            var configuration = new CaseConfiguration();
            configuration.Uri = new Uri("http://foo.bar");
            configuration.CredentialConfigurationKey = Guid.NewGuid().ToString();
            configuration.RequestCount = 1;
            var testCases = _httpTestContext.Execute(new List<TestCase>
            {
                new(configuration)
            }, null).Result;
        });
    }

    [Test]
    public void Add_Null_Test_Cases_Args_Should_Throw()
    {
        Assert.CatchAsync(async () => { await _httpTestContext.Execute(((IReadOnlyCollection<TestCase>) null!)!, null); });
    }

    [Test]
    public void Add_Null_Test_Case_Args_Should_Throw()
    {
        Assert.CatchAsync(async () => { await _httpTestContext.Execute(((TestCase) null!)!, null); });
    }

    [Test]
    public void Add_Credential_Has_Invalid_Provider_Name_Should_Throw()
    {
        Assert.Throws<AggregateException>(() =>
        {
            var testConfiguration = new TestConfiguration();
            testConfiguration.CredentialConfigurations = new[]
            {
                new CredentialConfiguration
                {
                    Data = new Dictionary<string, object>(),
                    Key = Guid.NewGuid().ToString(),
                    Provider = "NonExistingProviderName"
                }
            };

            var configuration = new CaseConfiguration();
            configuration.Uri = new Uri("http://foo.bar");
            configuration.CredentialConfigurationKey = Guid.NewGuid().ToString();
            testConfiguration.TestCaseConfigurations = new[]
            {
                new CaseConfiguration()
            };

            var testCases = testConfiguration.TestCaseConfigurations
                .Select(f => new TestCase(f))
                .ToList()
                .AsReadOnly();

            var testResult = _httpTestContext.Execute(testCases, testConfiguration.CredentialConfigurations).Result;
        });
    }

    [Test]
    public async Task Non_Existing_Url_Should_Return_Error()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = new Uri("http://non.existing.url:1234");
        caseConfiguration.RequestCount = 1;

        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);

        Assert.NotNull(allResults.Items.First().Errors);
        Assert.IsNotEmpty(allResults.Items.First().Errors);
        Assert.AreEqual(1, allResults.Items.First().Errors.Length);
    }

    [Test]
    public async Task LoadFromConfiguration_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 1;

        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Items.Count());

        var result = allResults.Items.First();
        Assert.NotNull(result);
        Assert.IsEmpty(result.Errors);
        Assert.IsNotEmpty(result.Timings);
        Assert.AreEqual(caseConfiguration.RequestCount, result.Timings.Count());
        Assert.NotZero(result.Elapsed.TotalSeconds);
    }

    [Test]
    public void Execute_With_Null_Configuration_Source_Should_Throw()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var allResults = await _httpTestContext.Execute((IConfigurationSource) null!);
        });
    }

    [Test]
    public void Execute_With_Null_Test_Configuration_Source_Should_Throw()
    {
        Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            var source = new MockConfigurationSource(new TestConfiguration() { TestCaseConfigurations = null! });
            var allResults = await _httpTestContext.Execute(source);
        });
    }

    [Test]
    public async Task Execute_With_TestCaseAndCredential_Should_Pass()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;
        caseConfiguration.CredentialConfigurationKey = "Mock";

        var credentialConfiguration = new CredentialConfiguration
        {
            Data = new Dictionary<string, object>
            {
                {"username", "test"},
                {"password", "test"}
            },
            Key = "Mock",
            Provider = nameof(MockCredentialProvider)
        };

        var testConfiguration = new TestConfiguration
        {
            TestCaseConfigurations = new[] {caseConfiguration},
            CredentialConfigurations = new[] {credentialConfiguration}
        };

        var allResults = await _httpTestContext.Execute(new MockConfigurationSource(testConfiguration));
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Items.Count());

        var result = allResults.Items.First();
        Assert.NotNull(result);
        Assert.IsNotNull(result.Errors);
        Assert.IsEmpty(result.Errors!);
        Assert.IsNotNull(result.Timings);
        Assert.AreEqual(caseConfiguration.RequestCount, result.Timings.Count());
        Assert.NotZero(result.Elapsed.TotalSeconds);

        caseConfiguration = null;
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _httpTestContext.Execute(caseConfiguration));
    }

    [Test]
    public async Task LoadFromConfigurationSource_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;

        var testConfiguration = new TestConfiguration
        {
            TestCaseConfigurations = new[] {caseConfiguration}
        };

        var configurationSource = new MockConfigurationSource(testConfiguration);
        var allResults = await _httpTestContext.Execute(configurationSource);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Items.Count());

        var result = allResults.Items.First();
        Assert.NotNull(result);
        Assert.IsNotNull(result.Errors);
        Assert.IsEmpty(result.Errors);
        Assert.IsNotNull(result.Timings);
        Assert.NotZero(result.Elapsed.TotalMilliseconds);       
        Assert.AreEqual(caseConfiguration.RequestCount, result.Timings.Count());
        Assert.NotZero(result.Elapsed.TotalSeconds);
    }

    [Test]
    public async Task LoadFromAnalyzeConfiguration_Should_Execute()
    {
        var configuration = new AnalyzeConfiguration();
        configuration.Uri = _uri;
        configuration.MaxTrialCount = 1;
        configuration.CalculationFunction = CalculationFunction.Median;

        var analyzeResult = await _httpTestContext.Analyze(configuration, new MockLogger());
        Assert.IsNotEmpty(analyzeResult.Results);
        Assert.NotZero(analyzeResult.MaxRequestsPerSecond);
        Assert.NotZero(analyzeResult.Results.First().Items.First().Elapsed.TotalMilliseconds);
    }

    [Test]
    public async Task Slow_Uri_Segmentation_Should_Work()
    {
        var configuration = new AnalyzeConfiguration();
        configuration.Uri = new Uri(_uri.ToString() + "slow");
        configuration.MaxTrialCount = 1;
        configuration.CalculationFunction = CalculationFunction.Median;

        var analyzeResult = await _httpTestContext.Analyze(configuration, new MockLogger());
        Assert.IsNotEmpty(analyzeResult.Results);
        Assert.NotZero(analyzeResult.MaxRequestsPerSecond);
        Assert.NotZero(analyzeResult.Results.First().Items.First().Elapsed.TotalMilliseconds);
    }

    [Test]
    public async Task LoadFromConfigurationHasBody_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.HttpMethod = "POST";
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;
        caseConfiguration.Body = new BodyConfiguration
        {
            Content = "test",
            ContentType = "text/plain",
            Encoding = Encoding.UTF8.WebName
        };

        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Items.Count());
        Assert.NotZero(allResults.Items.First().Elapsed.TotalMilliseconds);
    }

    [Test]
    public async Task BuildHttpClient_Should_Execute_With_HeaderCookieAndBody()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.Headers = new Dictionary<string, object> {{"foo", "bar"}};
        caseConfiguration.Cookies = new Dictionary<string, object> {{"foo", "bar"}};
        caseConfiguration.Body = new BodyConfiguration
        {
            Content = "foo",
            ContentType = "text/plain",
            Encoding = Encoding.UTF8.WebName
        };
        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Items.Count());
    }

    [TearDown]
    public void TearDown()
    {
        _server?.StopAsync().Wait();
    }
}