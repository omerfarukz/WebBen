using WebBen.Core.Extensions;

namespace WebBen.Core.Exporters;

public class TextWriterExporter : IExporter
{
    private readonly TextWriter _writer;

    public TextWriterExporter(TextWriter writer)
    {
        _writer = writer;
    }
    
    public void Export(IEnumerable<TestCase?> testCases)
    {
        _writer.Write(testCases!.AsTable());
    }

    public void Export(AnalyzeResult analyzeResult)
    {
        _writer.WriteLine(analyzeResult.Results.AsTable());
        _writer.WriteLine($"Best RPS is {analyzeResult.MaxRequestsPerSecond}");
    }
}