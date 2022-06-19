using System.CommandLine;
using NUnit.Framework;
using WebBen.CLI.CommandLine;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.CLI;

public class CommandTests
{
    [Test]
    public void AnalyzeCommand_Should_Parse_Args()
    {
        var command = new AnalyzeCommand(new MockLogger());

        var uri = "http://foo.bar";
        var parseResult = command.Parse($"uri -f -r -t 12 -m 34 -c P80");
        
        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);
    }
}