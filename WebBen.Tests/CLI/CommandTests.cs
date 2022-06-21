using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using WebBen.CLI.CommandLine;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.CLI;

public class CommandTests
{
    [Test]
    public async Task AnalyzeCommand_Should_Parse_Args()
    {
        var command = new AnalyzeCommand(new MockLogger());

        var uri = "http://foo.bar";
        var parseResult = command.Parse($"{uri} -f -r -t 12 -m 34 -c P80");

        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);
        
        
        Assert.NotNull(command.Handler);
        await command.Handler.InvokeAsync(new InvocationContext(parseResult));
    }

    [Test]
    public async Task ConfigCommand_Should_Parse_Args()
    {
        var command = new ConfigCommand(new MockLogger());
        var parseResult = command.Parse("assets/simple.json");

        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);

        await command.Handle(new FileInfo("assets/simple.json"));
    }

    [Test]
    public async Task UriCommand_Should_Parse_Args()
    {
        var command = new UriCommand(new MockLogger());

        var uri = "http://foo.bar";
        var parseResult = command.Parse($"{uri} -f -r -t 10 -m 20 -p 30");

        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);
        
        Assert.NotNull(command.Handler);
        await command.Handler.InvokeAsync(new InvocationContext(parseResult));
    }

    [Test]
    public void WebBenRootCommand_Should_Parse_Args()
    {
        var command = new WebBenRootCommand(new MockLogger());
        var parseResult = command.Parse("uri http://foo.bar");
        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);

        parseResult = command.Parse("config config.json");
        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);

        parseResult = command.Parse("analyze http://foo.bar");
        Assert.IsEmpty(parseResult.Errors);
        Assert.IsNull(parseResult.CommandResult.ErrorMessage);
    }
}