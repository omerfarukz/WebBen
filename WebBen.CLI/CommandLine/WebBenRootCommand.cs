using System.CommandLine;
using WebBen.Core.Configuration;
using WebBen.Core.Exporters;
using WebBen.Core.Logging;

namespace WebBen.CLI.CommandLine;

internal class WebBenRootCommand : RootCommand
{
    private readonly IExporter _exporter;
    
    public readonly Option<bool> VerboseOption;
    public readonly Option<Exporter> ExporterOption;
    
    public WebBenRootCommand(IExporter exporter, ILogger logger)
    {
        _exporter = exporter;
        AddCommand(new ConfigCommand(_exporter, logger));
        AddCommand(new UriCommand(_exporter, logger));
        AddCommand(new AnalyzeCommand(_exporter, logger));

        VerboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");
        ExporterOption = new Option<Exporter>(new[] {"--exporter", "-e"}, "Exporter option");
        AddGlobalOption(VerboseOption);
    }

}