using System.Net;

namespace WebBen.Common.CredentialProviders;

public interface ICredentialProvider
{
    ICredentials FromConfiguration(IDictionary<string, object> props);
}