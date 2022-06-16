using System.CommandLine;
using WebBen.CLI.CommandLine;
using WebBen.CLI.Common.Logging;

var logger = new ConsoleLogger();
AppDomain.CurrentDomain.UnhandledException += (s, e) => { logger.Error($"{e.ExceptionObject}"); };

// Build commands
var rootCommand = new RootCommand
{
    new ConfigCommand(logger),
    new UriCommand(logger),
    new AnalyzeCommand(logger)
};
var verboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");
rootCommand.AddGlobalOption(verboseOption);

// Invoke command
var parseResult = rootCommand.Parse(args);
var verbose = parseResult.GetValueForOption<bool>(verboseOption);
logger.Verbose = verbose;

await rootCommand.InvokeAsync(args);