using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class HttpTestContextTests
{
    private Uri _uri = null!;
    private WebApplication _server;
    private HttpTestContext _httpTestContext;

    [SetUp]
    public async Task Setup()
    {
        _server = MockWebApplication.CreateServer();
        _uri = new Uri(_server.Urls.First());
        _httpTestContext = new HttpTestContext(MockLogger.Instance);
        _httpTestContext.AddCredentialProvider(new MockCredentialProvider());
    }

    [Test]
    public async Task LoadFromConfiguration_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;

        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Count());

        var result = allResults.First();
        Assert.NotNull(result);
        Assert.IsEmpty(result.Errors);
        Assert.IsNotEmpty(result.Timings);
        Assert.AreEqual(caseConfiguration.RequestCount, result.Timings.Count());
        Assert.NotZero(result.Elapsed.TotalSeconds);
    }

    [Test]
    public async Task Execute_With_TestCaseAndCredential_Should_Pass()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;
        caseConfiguration.CredentialConfigurationKey = "Mock";

        var credentialConfiguration = new CredentialConfiguration()
        {
            Data = new Dictionary<string, object>()
            {
                {"username", "test"},
                {"password", "test"}
            },
            Key = "Mock",
            Provider = nameof(MockCredentialProvider)
        };

        var testConfiguration = new TestConfiguration()
        {
            TestCaseConfigurations = new[] {caseConfiguration},
            CredentialConfigurations = new[] {credentialConfiguration}
        };

        var allResults = await _httpTestContext.Execute(new MockConfigurationSource(testConfiguration));
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Count());

        var result = allResults.First();
        Assert.NotNull(result);
        Assert.IsEmpty(result.Errors);
        Assert.IsNotEmpty(result.Timings);
        Assert.AreEqual(caseConfiguration.RequestCount, result.Timings.Count());
        Assert.NotZero(result.Elapsed.TotalSeconds);
    }

    [Test]
    public async Task LoadFromConfigurationSource_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;

        var testConfiguration = new TestConfiguration()
        {
            TestCaseConfigurations = new[] {caseConfiguration}
        };

        var configurationSource = new MockConfigurationSource(testConfiguration);
        var allResults = await _httpTestContext.Execute(configurationSource);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Count());

        var result = allResults.First();
        Assert.NotNull(result);
        Assert.IsEmpty(result.Errors);
        Assert.IsNotEmpty(result.Timings);
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

        var RPS = await _httpTestContext.GetLastRPS(configuration, new MockLogger());
        Assert.NotZero(RPS);
    }

    [Test]
    public async Task LoadFromConfigurationHasBody_Should_Execute()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.HttpMethod = "POST";
        caseConfiguration.Uri = _uri;
        caseConfiguration.RequestCount = 3;
        caseConfiguration.Body = new BodyConfiguration()
        {
            Content = "test",
            ContentType = "text/plain",
            Encoding = Encoding.UTF8.WebName
        };

        var allResults = await _httpTestContext.Execute(caseConfiguration);
        Assert.NotNull(allResults);
        Assert.AreEqual(1, allResults.Count());
    }

    [TearDown]
    public void TearDown()
    {
        _server?.StopAsync().Wait();
    }
}