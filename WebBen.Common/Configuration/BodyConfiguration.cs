namespace WebBen.Common.Configuration;

public class BodyConfiguration
{
    public string Content { get; set; } = null!;
    public string ContentType { get; set; } = "text/plain";
    public string Encoding { get; set; } = System.Text.Encoding.UTF8.WebName;
}