namespace WebBen.Core.Configuration.Source;

public class FileConfigurationSource : IConfigurationSource
{
    private readonly string _filePath;

    public FileConfigurationSource(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException(nameof(filePath));
    }

    public async Task<string> GetContent()
    {
        return await File.ReadAllTextAsync(_filePath);
    }
}