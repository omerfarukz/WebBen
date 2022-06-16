using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using WebBen.CLI.Common;
using WebBen.CLI.Common.Logging;
using WebBen.CLI.Configuration;
using WebBen.CLI.Extensions;

namespace WebBen.CLI.CommandLine;

internal class UriCommand : Command
{
    private readonly ILogger _logger;

    public UriCommand(ILogger logger) : base("uri")
    {
        _logger = logger;
        AddArgument(new Argument<Uri>("uri", "The URI to use."));
        AddOption(new Option<int>(new[] {"-p", "--parallelism"}, "The number of parallelism to use."));
        AddOption(new Option<int>(new[] {"-b", "--bounded-capacity"}, "The bounded capacity to use."));
        AddOption(new Option<int>(new[] {"-t", "--timeout-in-ms"}, "The bounded capacity to use."));
        AddOption(new Option<string>(new[] {"-m", "--http-method"}, "The HTTP method to use."));
        AddOption(new Option<bool>(new[] {"-f", "--fetch-content"}, "Whether to fetch the content of the URI."));
        AddOption(new Option<bool>(new[] {"-r", "--allow-redirect"}, "Whether to allow redirects."));
        AddOption(new Option<int>(new[] {"-n", "--request-count"}, "The number of requests to make."));
        AddOption(new Option<int>(new[] {"-s", "--max-buffer-size"}, "The maximum size of the response content buffer."));

        Handler = CommandHandler.Create(Handle);
    }

    private async Task Handle(CaseConfiguration configuration)
    {
        var context = new HttpTestContext(_logger);
        var result = await context.Execute(configuration);
        _logger.Info(result.AsTable());
    }
}