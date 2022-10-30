using System.Threading.Tasks;
using NUnit.Framework;
using WebBen.CLI;

namespace WebBen.Tests.CLI;

public class ProgramTests
{
    [Test]
    public async Task AnalyzeCommand_Should_Parse_Args()
    {
        await Program.Main(new[] {"--help"});
        Assert.Pass();
    }
}