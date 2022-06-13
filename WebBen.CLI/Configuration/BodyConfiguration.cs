namespace WebBen.CLI.Configuration;

public class BodyConfiguration
{
    public string Content { get; set; }
    public string ContentType { get; set; }
    public string Encoding { get; set; } = System.Text.Encoding.UTF8.WebName;
}