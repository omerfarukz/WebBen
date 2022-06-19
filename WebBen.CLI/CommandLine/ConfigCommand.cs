using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.Core;
using WebBen.Core.Configuration.Source;
using WebBen.Core.Logging;
using WebBen.Core.Extensions;

namespace WebBen.CLI.CommandLine;

internal class ConfigCommand : Command
{
    private readonly ILogger _logger;
    public const string CommandName = "config";

    public ConfigCommand(ILogger logger) : base(CommandName)
    {
        AddArgument(new Argument<FileInfo>("fileInfo", "The file to read the configuration from"));
        Handler = CommandHandler.Create(Handle);
        _logger = logger;
    }

    private async Task Handle(FileInfo fileInfo)
    {
        var context = new HttpTestContext(_logger);
        var result = await context.Execute(new FileConfigurationSource(fileInfo.FullName));
        _logger.Info(result.AsTable());
    }
}