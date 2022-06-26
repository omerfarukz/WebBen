using WebBen.Core.Exporters;
using WebBen.Core.Results;

namespace WebBen.Tests.Mocks;

public class MockExporter : IExporter
{
    public void Export(TestResult testResult)
    {
    }

    public void Export(AnalyzeResult analyzeResult)
    {
    }
}