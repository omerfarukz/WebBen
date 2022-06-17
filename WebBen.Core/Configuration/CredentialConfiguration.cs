using WebBen.Core.CredentialProviders;

namespace WebBen.Core.Configuration;

public class CredentialConfiguration
{
    public string Key { get; set; } = null!;
    public string Provider { get; set; } = nameof(NetworkCredentialProvider);
    public Dictionary<string, object>? Data { get; set; }
}