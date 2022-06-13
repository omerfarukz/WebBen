namespace WebBen.CLI.Configuration;

public class Configuration
{
    public CaseConfiguration[] TestCases { get; set; } = null!;

    public CredentialConfiguration[]? CredentialConfigs { get; set; }
}