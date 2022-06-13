using System.CommandLine;
using WebBen.CLI.CommandLine;
using WebBen.CLI.Common;

var configCommand = new ConfigCommand(async fileInfo =>
{
    var context = new HttpTestContext();
    await context.Execute(fileInfo);
});
var uriCommand = new UriCommand(async c =>
{
    var context = new HttpTestContext();
    await context.Execute(c);
});

var rootCommand = new RootCommand {configCommand, uriCommand};
await rootCommand.InvokeAsync(args);