using System;
using NUnit.Framework;
using WebBen.Core.Configuration;
using WebBen.Core.Exporters;
using WebBen.Core.Results;

namespace WebBen.Tests.Common;

public class TextWriterExporterTests : TextWriterTestBase
{
    [Test]
    public void Exporter_TestResultItem_Should_Write_Console_Out()
    {
        var exporter = new TextWriterExporter(StreamWriter);
        var testResult = new TestResult(new[]
        {
            new TestResultItem()
            {
                Configuration = new CaseConfiguration()
                {
                    Name = "test",
                },
                Timings = Array.Empty<TimeSpan>(),
                Errors = Array.Empty<string>()
            },
        });

        exporter.Format = ExportFormat.Default;
        exporter.Export(testResult);
        var actual = ReadAndClearConsoleOut();
        Assert.NotZero(actual.Length);

        exporter.Format = ExportFormat.Json;
        exporter.Export(testResult);
        actual = ReadAndClearConsoleOut();
        Assert.NotZero(actual.Length);

        exporter.Format = null;
        Assert.Throws<NotSupportedException>(() => exporter.Export(testResult), "Format is not supported");
    }

    [Test]
    public void Exporter_AnalyzeResult_Should_Write_Console_Out()
    {
        var exporter = new TextWriterExporter(StreamWriter);
        var testResult = new AnalyzeResult(new []{ new TestResult(new[]
        {
            new TestResultItem()
            {
                Configuration = new CaseConfiguration()
                {
                    Name = "test",
                },
                Timings = Array.Empty<TimeSpan>(),
                Errors = Array.Empty<string>()
            },
        })});

        exporter.Format = ExportFormat.Default;
        exporter.Export(testResult);
        var actual = ReadAndClearConsoleOut();
        Assert.NotZero(actual.Length);

        exporter.Format = ExportFormat.Json;
        exporter.Export(testResult);
        actual = ReadAndClearConsoleOut();
        Assert.NotZero(actual.Length);

        exporter.Format = null;
        Assert.Throws<NotSupportedException>(() => exporter.Export(testResult), "Format is not supported");
    }
}