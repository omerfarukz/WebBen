using System.Net;

namespace WebBen.CLI.CredentialProviders
{
    internal interface ICredentialProvider
    {
        ICredentials FromConfiguration(IDictionary<string, object> props);
    }
}