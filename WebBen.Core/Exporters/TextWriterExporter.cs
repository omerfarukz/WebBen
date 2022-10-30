using System.Text.Json;
using System.Text.Json.Serialization;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;
using WebBen.Core.Results;

namespace WebBen.Core.Exporters;

public class TextWriterExporter : IExporter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly TextWriter _writer;

    public TextWriterExporter(TextWriter writer)
    {
        _writer = writer;
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true, MaxDepth = int.MaxValue, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public ExportFormat? Format { get; set; }

    public void Export(TestResult testResult)
    {
        testResult.BuildSummaries();
        switch (Format)
        {
            case ExportFormat.Default:
                _writer.Write(AsTable(testResult));
                break;
            case ExportFormat.Json:
                _writer.Write(JsonSerializer.Serialize(testResult, _jsonSerializerOptions));
                break;
            default:
                throw new NotSupportedException("Format is not supported");
        }
    }

    public void Export(AnalyzeResult analyzeResult)
    {
        analyzeResult.BuildSummaries();
        switch (Format)
        {
            case ExportFormat.Default:
                _writer.WriteLine(AsTable(analyzeResult));
                break;
            case ExportFormat.Json:
                _writer.Write(JsonSerializer.Serialize(analyzeResult, _jsonSerializerOptions));
                break;
            default:
                throw new NotSupportedException("Format is not supported");
        }
    }

    private static string AsTable(TestResult testResult)
    {
        var asTable = testResult.Items.ToStringTable(
            new[] {"Name", "Elapsed", "NoR", "Pll", "Err", "Avg(ms)", "StdDev(ms)", "Median(ms)"},
            f => f.Configuration!.Name ?? DateTime.Now.ToString("yyMMddHHmmssffff"),
            f => f.Elapsed.TotalSeconds.ToString("N"),
            f => f.Configuration!.RequestCount.ToString(),
            f => f.Configuration!.Parallelism.ToString(),
            f => f.Errors!.Length.ToString(),
            f => f.Calculations![CalculationFunction.Average].TotalMilliseconds.ToString("N"),
            f => f.Calculations![CalculationFunction.StdDev].TotalMilliseconds.ToString("N"),
            f => f.Calculations![CalculationFunction.Median].TotalMilliseconds.ToString("N")
        );

        return asTable;
    }

    private static string AsTable(AnalyzeResult analyzeResult)
    {
        var resultSetAsTable = analyzeResult.Results.SelectMany(f => f.Items).ToStringTable(
            new[] {"Name", "Elapsed(sec)", "NoR", "Pll", "Err", "Avg(ms)", "StdDev(ms)", "Median(ms)"},
            f => f.Configuration!.Name ?? DateTime.Now.ToString("yyMMddHHmmssffff"),
            f => f.Elapsed.TotalSeconds.ToString("N"),
            f => f.Configuration!.RequestCount,
            f => f.Configuration!.Parallelism,
            f => f.Errors!.Length.ToString(),
            f => f.Calculations![CalculationFunction.Average].TotalMilliseconds.ToString("N"),
            f => f.Calculations![CalculationFunction.StdDev].TotalMilliseconds.ToString("N"),
            f => f.Calculations![CalculationFunction.Median].TotalMilliseconds.ToString("N")
        );

        var analysisAsTable = new[] {analyzeResult}.ToStringTable(
            new[] {"MaxRPS"},
            f => f.MaxRequestsPerSecond
        );
        return string.Concat(resultSetAsTable, analysisAsTable);
    }
}