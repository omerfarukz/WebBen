using System.Collections.Generic;
using WebBen.Core;
using WebBen.Core.Exporters;
using WebBen.Core.Extensions;

namespace WebBen.Tests.Mocks;

public class MockExporter : IExporter
{
    public void Export(IEnumerable<TestCase> testCases)
    {
        throw new System.NotImplementedException();
    }

    public void Export(AnalyzeResult testCases)
    {
        throw new System.NotImplementedException();
    }
}