using System.CommandLine;
using WebBen.Core.Logging;

namespace WebBen.CLI.CommandLine;

internal class WebBenRootCommand : RootCommand
{
    public readonly Option<bool> VerboseOption;
    
    public WebBenRootCommand(ILogger logger)
    {
        AddCommand(new ConfigCommand(logger));
        AddCommand(new UriCommand(logger));
        AddCommand(new AnalyzeCommand(logger));

        VerboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");
        AddGlobalOption(VerboseOption);
    }
}