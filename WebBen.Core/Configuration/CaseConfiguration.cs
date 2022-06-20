namespace WebBen.Core.Configuration;

public class CaseConfiguration : ConfigurationBase
{
    public string HttpMethod { get; set; } = System.Net.Http.HttpMethod.Get.Method;
    public int RequestCount { get; set; } = 100;
    public int Parallelism { get; set; } = 100;
    public bool UseDefaultCredentials { get; set; }
    public bool UseCookieContainer { get; set; }

    /// <summary>
    ///     MaxResponseContent is in bytes
    /// </summary>
    public int MaxBufferSize { get; set; } = int.MaxValue;

    public string? CredentialConfigurationKey { get; set; }
    public Dictionary<string, object>? Headers { get; set; }
    public Dictionary<string, object>? Cookies { get; set; }
    public BodyConfiguration? Body { get; set; }
}