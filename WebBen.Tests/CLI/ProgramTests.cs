using System.Threading.Tasks;
using NUnit.Framework;
using static WebBen.CLI.Program;

namespace WebBen.Tests.CLI;

public class ProgramTests
{
    [Test]
    public async Task Program_Should_Execute_Without_Error()
    {
        await Main(new []{"--help"});
        await Main(new []{"--verbose"});
        await Main(new []{"uri"});
        await Main(new []{"config"});
        await Main(new []{"analyze"});
    }
}