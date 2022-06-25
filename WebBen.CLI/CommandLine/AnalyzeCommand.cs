using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.Core;
using WebBen.Core.Configuration;
using WebBen.Core.Exporters;
using WebBen.Core.Extensions;
using WebBen.Core.Logging;

namespace WebBen.CLI.CommandLine;

internal class AnalyzeCommand : Command
{
    private const string CommandName = "analyze";
    private readonly IExporter _exporter;
    private readonly ILogger _logger;

    public AnalyzeCommand(IExporter exporter, ILogger logger) : base(CommandName)
    {
        _exporter = exporter;
        _logger = logger;

        AddAlias("analyse");
        AddArgument(new Argument<Uri>("uri", "The URI to use."));
        AddOption(new Option<bool>(new[] {"-f", "--fetch-content"}, "Whether to fetch the content of the URI."));
        AddOption(new Option<bool>(new[] {"-r", "--allow-redirect"}, "Whether to allow redirects."));
        AddOption(new Option<int>(new[] {"-t", "--timeout-in-ms"}, "The bounded capacity to use."));
        AddOption(new Option<int>(new[] {"-m", "--max-trial-count"}, "Iteration count for calculation. See -c"));
        AddOption(new Option<CalculationFunction>(new[] {"-c", "--calculation-function"},
            "Function for RPS calculation"));

        Handler = CommandHandler.Create(Handle);
    }

    private async Task Handle(AnalyzeConfiguration configuration)
    {
        var context = new HttpTestContext(_logger);
        var analyzeResult = await context.Analyze(configuration, _logger);
        _exporter.Export(analyzeResult);
    }
}