namespace WebBen.CLI.Configuration;

public class TestConfiguration
{
    public TestCaseConfiguration[] TestCases { get; set; } = null!;

    public CredentialConfiguration[]? CredentialConfigs { get; set; }
}