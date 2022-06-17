using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;
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