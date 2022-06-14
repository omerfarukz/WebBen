using System.CommandLine;
using WebBen.CLI.CommandLine;
using WebBen.CLI.Common;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Extensions;

var logger = new ConsoleLogger();

var configCommand = new ConfigCommand(async fileInfo =>
{
    var context = new HttpTestContext(logger);
    var result = await context.Execute(fileInfo);
    logger.Log(result.AsTable());
});
var uriCommand = new UriCommand(async configuration =>
{
    var context = new HttpTestContext(logger);
    var result = await context.Execute(configuration);
    logger.Log(result.AsTable());
});

var rootCommand = new RootCommand {configCommand, uriCommand};
rootCommand.Invoke(" config Simple.json");
// await rootCommand.InvokeAsync(args);