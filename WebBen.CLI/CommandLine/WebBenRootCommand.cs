using System.CommandLine;
using WebBen.Core.Configuration;
using WebBen.Core.Exporters;
using WebBen.Core.Logging;

namespace WebBen.CLI.CommandLine;

internal class WebBenRootCommand : RootCommand
{
    public readonly Option<bool> VerboseOption;
    public readonly Option<ExportFormat> ExportFormatOption;
    
    public WebBenRootCommand(IExporter exporter, ILogger logger)
    {
        AddCommand(new ConfigCommand(exporter, logger));
        AddCommand(new UriCommand(exporter, logger));
        AddCommand(new AnalyzeCommand(exporter, logger));

        VerboseOption = new Option<bool>(new[] {"--verbose", "-v"}, "Enable verbose output");
        ExportFormatOption = new Option<ExportFormat>(new[] {"--export-format", "-e"}, "Exporter option");
        ExportFormatOption.SetDefaultValue(ExportFormat.Default);
        
        AddGlobalOption(VerboseOption);
        AddGlobalOption(ExportFormatOption);
    }

}