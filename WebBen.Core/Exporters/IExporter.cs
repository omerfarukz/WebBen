using WebBen.Core.Extensions;
using WebBen.Core.Results;

namespace WebBen.Core.Exporters;

public interface IExporter
{
    void Export(TestResult testResult);
    void Export(AnalyzeResult analyzeResult);
}