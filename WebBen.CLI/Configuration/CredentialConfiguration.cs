using WebBen.CLI.CredentialProviders;

namespace WebBen.CLI.Configuration;

public class CredentialConfiguration
{
    public string Key { get; set; } = null!;
    public string Provider { get; set; } = nameof(NetworkCredentialProvider);
    public Dictionary<string, object>? Data { get; set; }
}