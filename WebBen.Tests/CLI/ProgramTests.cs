using System.CommandLine.Invocation;
using System.Threading.Tasks;
using NUnit.Framework;
using WebBen.CLI;
using WebBen.Core.Logging;

namespace WebBen.Tests.CLI;

public class ProgramTests : TextWriterTestBase
{
    [Test]
    public async Task AnalyzeCommand_Should_Parse_Args()
    {
        var logger = new TextWriterLogger(StreamWriter);
        await Program.Main(new[] {"--help"});

        Assert.Pass();
    }
}