using System.Text.Json;
using System.Text.Json.Serialization;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;

namespace WebBen.Core.Exporters;

public class TextWriterExporter : IExporter
{
    private readonly TextWriter _writer;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    public ExportFormat Format { get; set; }

    public TextWriterExporter(TextWriter writer)
    {
        _writer = writer;
        _jsonSerializerOptions = new JsonSerializerOptions() {WriteIndented = true, MaxDepth = int.MaxValue};
    }


    public void Export(IEnumerable<TestCase?> testCases)
    {
        switch (Format)
        {
            case ExportFormat.Default:
                _writer.Write(testCases!.AsTable());
                break;
            case ExportFormat.Json:
                _writer.Write(JsonSerializer.Serialize(testCases, _jsonSerializerOptions));
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void Export(AnalyzeResult analyzeResult)
    {
        switch (Format)
        {
            case ExportFormat.Default:
                _writer.WriteLine(analyzeResult.Results.AsTable());
                _writer.WriteLine($"Best RPS is {analyzeResult.MaxRequestsPerSecond}");
                break;
            case ExportFormat.Json:
                _writer.Write(JsonSerializer.Serialize(analyzeResult));
                break;
            default:
                throw new NotImplementedException();
        }
        
    }
}