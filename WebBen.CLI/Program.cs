using System.CommandLine;
using WebBen.CLI.CommandLine;
using WebBen.Core.Exporters;
using WebBen.Core.Logging;

namespace WebBen.CLI;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = new TextWriterLogger(Console.Out);
        var exporter = new TextWriterExporter(Console.Out);
        var rootCommand = new WebBenRootCommand(exporter, logger);

        // Invoke command
        var parseResult = rootCommand.Parse(args);
        logger.Verbose = parseResult.GetValueForOption(rootCommand.VerboseOption);
        exporter.Format = parseResult.GetValueForOption(rootCommand.ExportFormatOption);
        await rootCommand.InvokeAsync(args);
    }
}
