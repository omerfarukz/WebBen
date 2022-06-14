namespace WebBen.CLI.Configuration;

internal class TestConfiguration
{
    public CaseConfiguration[] TestCases { get; set; } = null!;

    public CredentialConfiguration[]? CredentialConfigs { get; set; }
}