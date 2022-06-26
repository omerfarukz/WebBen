using WebBen.Core.Logging;

namespace WebBen.Tests.Mocks;

public class MockLogger : ILogger
{
    public static readonly ILogger Instance = new MockLogger();
    public string Log { get; private set; }

    public void Info(string message)
    {
        Log = string.Concat(Log, "Info: ", message, "\r\n");
    }

    public void Debug(string message)
    {
        Log = string.Concat(Log, "Debug: ", message, "\r\n");
    }

    public void Error(string message)
    {
        Log = string.Concat(Log, "Error: ", message, "\r\n");
    }
}