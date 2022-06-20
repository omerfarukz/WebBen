using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using WebBen.CLI;

namespace WebBen.Tests.CLI;

public class ConsoleLoggerTests
{
    private MemoryStream _memoryStream;
    private StreamWriter _streamWriter;

    [SetUp]
    public void Setup()
    {
        _memoryStream = new MemoryStream();
        _streamWriter = new StreamWriter(_memoryStream, Encoding.ASCII);
    }

    [Test]
    public void Log_Should_Write_Console_Out()
    {
        var logger = new ConsoleLogger(_streamWriter);
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

    private string ReadAndClearConsoleOut()
    {
        _streamWriter.Flush();
        var text = Encoding.ASCII.GetString(_memoryStream.ToArray());
        _memoryStream.SetLength(0);
        return text;
    }
}