namespace WebBen.Core.Configuration.Source;

public class FileConfigurationSource : IConfigurationSource
{
    private readonly string _filePath;

    public FileConfigurationSource(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public async Task<string> GetContent()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException(nameof(_filePath));

        return await File.ReadAllTextAsync(_filePath);
    }
}