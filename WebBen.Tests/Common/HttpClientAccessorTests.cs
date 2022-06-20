using System;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class HttpClientAccessorTests
{
    [Test]
    public void Create_New_Without_Credential_TestCase_Should_Pass()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = new Uri("http://localhost");

        var testCase = new TestCase(caseConfiguration);
        var httpClientAccessor = new HttpClientAccessor(testCase, null);

        Assert.NotNull(httpClientAccessor.Client);
        Assert.Null(httpClientAccessor.CookieContainer);

        httpClientAccessor.Dispose();
    }

    [Test]
    public void Cookie_Container_Should_Not_Be_Null()
    {
        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = new Uri("http://localhost");
        caseConfiguration.UseCookieContainer = true;

        var testCase = new TestCase(caseConfiguration);
        var httpClientAccessor = new HttpClientAccessor(testCase, null);

        Assert.NotNull(httpClientAccessor.Client);
        Assert.NotNull(httpClientAccessor.CookieContainer);

        httpClientAccessor.Dispose();
    }

    [Test]
    public void Create_New_With_Credential_TestCase_Should_Pass()
    {
        var caseConfigurationUri = new Uri("http://localhost");
        var caseConfigurationTimeoutInSeconds = 3;

        var caseConfiguration = new CaseConfiguration();
        caseConfiguration.Uri = caseConfigurationUri;
        caseConfiguration.TimeoutInMs = caseConfigurationTimeoutInSeconds * 1000;

        var testCase = new TestCase(caseConfiguration);
        var httpClientAccessor = new HttpClientAccessor(testCase, new MockCredential());

        Assert.NotNull(httpClientAccessor.Client);
        Assert.Null(httpClientAccessor.CookieContainer);
        Assert.AreEqual(caseConfigurationUri, httpClientAccessor.Client.BaseAddress);
        Assert.AreEqual(caseConfigurationTimeoutInSeconds, httpClientAccessor.Client.Timeout.TotalSeconds);

        httpClientAccessor.Dispose();
    }
}