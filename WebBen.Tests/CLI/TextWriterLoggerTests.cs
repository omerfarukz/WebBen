using System;
using NUnit.Framework;
using WebBen.Core.Logging;

namespace WebBen.Tests.CLI;

public class TextWriterLoggerTests : TextWriterTestBase
{
    [Test]
    public void Log_Should_Write_Console_Out()
    {
        var logger = new TextWriterLogger(StreamWriter);
        var message = Guid.NewGuid().ToString();
        logger.Debug(message);

        var actual = ReadAndClearConsoleOut();
        Assert.AreEqual(0, actual.Length);

        logger.Verbose = true;
        logger.Debug(message);
        actual = ReadAndClearConsoleOut();
        Assert.AreEqual($"Debug: {message}\n", actual);

        message = Guid.NewGuid().ToString();
        logger.Info(message);
        actual = ReadAndClearConsoleOut();
        Assert.AreEqual($"Info: {message}\n", actual);

        logger.Error(message);
        actual = ReadAndClearConsoleOut();
        Assert.AreEqual($"Error: {message}\n", actual);
    }
}