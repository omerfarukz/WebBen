using System;
using System.Collections.Generic;
using System.Net;
using WebBen.Core.CredentialProviders;

namespace WebBen.Tests.Mocks;

public class MockCredentialProvider : ICredentialProvider
{
    public ICredentials FromConfiguration(IDictionary<string, object> props)
    {
        return new MockCredential();
    }
}

public class MockCredentialProvider2 : ICredentialProvider
{
    public ICredentials FromConfiguration(IDictionary<string, object> props)
    {
        return new MockCredential();
    }
}

public class MockCredential : ICredentials
{
    public NetworkCredential GetCredential(Uri uri, string authType)
    {
        return null;
    }
}