using System.CommandLine;
using WebBen.CLI.CommandLine;
using WebBen.CLI.Common.Logging;

// int workerThreads;
// int portThreads;
//
// ThreadPool.GetMaxThreads(out workerThreads, out portThreads);

// TODO: use core hosting
var logger = new ConsoleLogger();
AppDomain.CurrentDomain.UnhandledException += (sender, unhandledExceptionEventArgs) =>
{
    logger.Error($"{unhandledExceptionEventArgs.ExceptionObject}");
};

var configCommand = new ConfigCommand(logger);
var uriCommand = new UriCommand(logger);
var analyzeCommand = new AnalyzeCommand(logger);
 
var verboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");

var rootCommand = new RootCommand {configCommand, uriCommand, analyzeCommand};
rootCommand.AddGlobalOption(verboseOption);

var parseResult = rootCommand.Parse(args);
var verbose = parseResult.GetValueForOption<bool>(verboseOption);
logger.Verbose = verbose;
// await parseResult.InvokeAsync();
await rootCommand.InvokeAsync(args); // TODO: avoid double parsing