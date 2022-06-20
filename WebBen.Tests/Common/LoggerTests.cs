using NUnit.Framework;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class LoggerTests
{
    [Test]
    public void ShouldLogWithAllLevels()
    {
        var logger = new MockLogger();
        logger.Debug(string.Empty);
        logger.Error(string.Empty);
        logger.Info(string.Empty);
    }
}