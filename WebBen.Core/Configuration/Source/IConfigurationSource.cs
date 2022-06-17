namespace WebBen.Core.Configuration.Source;

public interface IConfigurationSource
{
    Task<string> GetContent();
}