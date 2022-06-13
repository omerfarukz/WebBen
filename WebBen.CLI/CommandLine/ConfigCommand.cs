using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace WebBen.CLI.CommandLine;

public class ConfigCommand : Command
{
    public ConfigCommand(Func<FileInfo, Task> handler) : base("config")
    {
        AddArgument(new Argument<FileInfo>("fileInfo", "The file to read the configuration from"));
        Handler = CommandHandler.Create<FileInfo>(handler);
    }
}