using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.CredentialProviders;
using WebBen.Core.Extensions;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class NetworkCredentiaProviderlTests
{
    [Test]
    public void Test_GetCredentials_Returns_Credentials()
    {
        var provider = new NetworkCredentialProvider();
        var username = "username";
        var password = "password";
        var credentials = provider.FromConfiguration(new Dictionary<string, object>()
        {
            {"username", username},
            {"password", password}
        });
        Assert.NotNull(credentials);
        var credential = credentials.GetCredential(new Uri("http://test.com"), "Negotiate");
        Assert.AreEqual(username, credential.UserName);
        Assert.AreEqual(password, credential.Password);
    }

    [Test]
    public void From_Configuration_Should_Throw_Exception_When_Username_Or_Password_Is_Not_Provided()
    {
        var provider = new NetworkCredentialProvider();
        Assert.Throws<InvalidDataException>(() => provider.FromConfiguration(
            new Dictionary<string, object>()
            {
                {"username", "username"}
            }), "username or password node is null or empty");
        
        provider = new NetworkCredentialProvider();
        Assert.Throws<InvalidDataException>(() => provider.FromConfiguration(
            new Dictionary<string, object>()
            {
                {"password", "password"}
            }), "username or password node is null or empty");
        
        provider = new NetworkCredentialProvider();
        Assert.Throws<InvalidDataException>(() => provider.FromConfiguration(
            new Dictionary<string, object>()
            { }), "username or password node is null or empty");

    }
}