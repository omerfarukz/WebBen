using System.Net;

namespace WebBen.Core.CredentialProviders;

public class NetworkCredentialProvider : ICredentialProvider
{
    public ICredentials FromConfiguration(IDictionary<string, object> props)
    {
        if (!props.ContainsKey("username") || !props.ContainsKey("password"))
            throw new InvalidDataException("username or password node is null or empty");

        var username = props["username"] as string;
        var password = props["password"] as string;
        var domain = props["domain"] as string;

        return new NetworkCredential(username, password, domain);
    }
}