using System.CommandLine;
using WebBen.CLI.CommandLine;

namespace WebBen.CLI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = new ConsoleLogger(Console.Out);
        // AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        // {
        //     logger.Error($"{e.ExceptionObject}");
        // };

        var rootCommand = new WebBenRootCommand(logger);

        // Invoke command
        var parseResult = rootCommand.Parse(args);
        var verbose = parseResult.GetValueForOption(rootCommand.VerboseOption);
        logger.Verbose = verbose;

        await rootCommand.InvokeAsync(args);
    }
}