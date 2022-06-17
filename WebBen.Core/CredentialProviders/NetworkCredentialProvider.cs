using System.Net;

namespace WebBen.Core.CredentialProviders;

public class NetworkCredentialProvider : ICredentialProvider
{
    public ICredentials FromConfiguration(IDictionary<string, object> props)
    {
        if (!props.ContainsKey("username") || !props.ContainsKey("password"))
            throw new InvalidDataException("username or password node is null or empty");

        var username = props["username"]?.ToString();
        var password = props["password"]?.ToString();

        return new NetworkCredential(username, password);
    }
}