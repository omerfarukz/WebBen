namespace WebBen.CLI.Configuration;

internal class ConfigurationBase
{
    public string? Name { get; set; }
    public Uri? Uri { get; set; }
    public bool FetchContent { get; set; }
    public bool AllowRedirect { get; set; }
    public int TimeoutInMs { get; set; } = int.MaxValue;
}