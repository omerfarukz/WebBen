using System.CommandLine;
using System.CommandLine.Parsing;
using WebBen.CLI.CommandLine;
using WebBen.CLI.Common;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

var logger = new ConsoleLogger();
AppDomain.CurrentDomain.UnhandledException += (sender, unhandledExceptionEventArgs) =>
{
    logger.Error($"{unhandledExceptionEventArgs.ExceptionObject}");
};

var configCommand = new ConfigCommand(async (FileInfo fileInfo) =>
{
    var context = new HttpTestContext(logger);
    var result = await context.Execute(fileInfo);
    logger.Info(result.AsTable());
});
var uriCommand = new UriCommand(async (CaseConfiguration configuration) =>
{
    var context = new HttpTestContext(logger);
    var result = await context.Execute(configuration);
    logger.Info(result.AsTable());
});

var verboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");

var rootCommand = new RootCommand {configCommand, uriCommand};
rootCommand.AddGlobalOption(verboseOption);

var parseResult = rootCommand.Parse(args);

var verbose = parseResult.GetValueForOption<bool>(verboseOption);
logger.Verbose = verbose;
await parseResult.InvokeAsync();