using NUnit.Framework;
using WebBen.Tests.Mocks;

namespace WebBen.Tests.Common;

public class LoggerTests
{
    [Test]
    public void ShouldLogWithAllLevels()
    {
        var logger = new MockLogger();
        logger.Debug("a");
        logger.Error("b");
        logger.Info("c");
        Assert.AreEqual("Debug: a\r\nError: b\r\nInfo: c\r\n", logger.Log);
    }
} 