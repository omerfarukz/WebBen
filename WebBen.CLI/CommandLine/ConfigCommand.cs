using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.Common;
using WebBen.Common.Logging;
using WebBen.Common.Extensions;

namespace WebBen.CLI.CommandLine;

internal class ConfigCommand : Command
{
    private readonly ILogger _logger;

    public ConfigCommand(ILogger logger) : base("config")
    {
        _logger = logger;
        AddArgument(new Argument<FileInfo>("fileInfo", "The file to read the configuration from"));
        Handler = CommandHandler.Create(Handle);
    }

    private async Task Handle(FileInfo fileInfo)
    {
        var context = new HttpTestContext(_logger);
        var result = await context.Execute(fileInfo);
        _logger.Info(result.AsTable());
    }
}