namespace WebBen.CLI.Configuration;

internal class CaseConfiguration
{
    public string? Name { get; set; } = $"{Guid.NewGuid()}";
    public Uri? Uri { get; set; }
    public string HttpMethod { get; set; } = System.Net.Http.HttpMethod.Get.Method;
    public int RequestCount { get; set; } = 100;
    public bool FetchContent { get; set; }
    public int Parallelism { get; set; } = 100;
    public int BoundedCapacity { get; set; } = 100;
    public bool UseDefaultCredentials { get; set; }
    public bool UseCookieContainer { get; set; }
    public bool AllowAutoRedirect { get; set; } = false;
    
    /// <summary>
    /// MaxResponseContent is in bytes
    /// </summary>
    public int MaxBufferSize { get; set; } = int.MaxValue;
    public int TimeoutInMs { get; set; } = int.MaxValue;
    public string? CredentialConfigurationKey { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
    public Dictionary<string, object>? Cookies { get; set; }
    public BodyConfiguration? Body { get; set; }
}