using System.Text;

namespace WebBen.CLI.Configuration;

public class TestCaseConfiguration
{
    public string? Name { get; set; } = $"{Guid.NewGuid()}";
    public Uri? Uri { get; set; }
    public string HttpMethod { get; set; } = System.Net.Http.HttpMethod.Get.Method;
    public int NumberOfRequests { get; set; } = 100;
    public bool FetchContent { get; set; }
    public int Parallelism { get; set; } = 100;
    public int BoundedCapacity { get; set; } = 100;
    public bool UseDefaultCredentials { get; set; }
    public bool UseCookieContainer { get; set; }
    public string? CredentialConfigurationKey { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
    public TestCaseBodyConfiguration? Body { get; set; }
}

public class TestCaseBodyConfiguration
{
    public string Content { get; set; }
    public string ContentType { get; set; }
    public string Encoding { get; set; } = System.Text.Encoding.UTF8.WebName;
}