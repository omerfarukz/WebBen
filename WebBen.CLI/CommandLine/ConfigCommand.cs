using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.Core;
using WebBen.Core.Configuration.Source;
using WebBen.Core.Extensions;
using WebBen.Core.Logging;

namespace WebBen.CLI.CommandLine;

internal class ConfigCommand : Command
{
    public const string CommandName = "config";
    private readonly ILogger _logger;

    public ConfigCommand(ILogger logger) : base(CommandName)
    {
        AddArgument(new Argument<FileInfo>("fileInfo", "The file to read the configuration from"));
        Handler = CommandHandler.Create(Handle);
        _logger = logger;
    }

    internal async Task Handle(FileInfo fileInfo)
    {
        var context = new HttpTestContext(_logger);
        var result = await context.Execute(new FileConfigurationSource(fileInfo.FullName));
        Console.WriteLine(result.AsTable());
    }
}