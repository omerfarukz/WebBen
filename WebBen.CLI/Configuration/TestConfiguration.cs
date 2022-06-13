namespace WebBen.CLI.Configuration;

public class TestConfiguration
{
    public CaseConfiguration[] TestCases { get; set; } = null!;

    public CredentialConfiguration[]? CredentialConfigs { get; set; }
}