using WebBen.Core.Extensions;

namespace WebBen.Core.Exporters;

public interface IExporter
{
    void Export(IEnumerable<TestCase?> testCases);
    void Export(AnalyzeResult testCases);
}