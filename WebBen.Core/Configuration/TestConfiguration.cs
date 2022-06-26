namespace WebBen.Core.Configuration;

public class TestConfiguration
{
    public CaseConfiguration?[] TestCaseConfigurations { get; set; } = null!;

    public CredentialConfiguration[]? CredentialConfigurations { get; set; }
}