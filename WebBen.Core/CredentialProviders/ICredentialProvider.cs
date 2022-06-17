using System.Net;

namespace WebBen.Core.CredentialProviders;

public interface ICredentialProvider
{
    ICredentials FromConfiguration(IDictionary<string, object> props);
}