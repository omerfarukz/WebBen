using WebBen.Core.Logging;

namespace WebBen.Tests.Mocks;

public class MockLogger : ILogger
{
    public static ILogger Instance = new MockLogger();

    public void Info(string message)
    {
    }

    public void Debug(string message)
    {
    }

    public void Error(string message)
    {
    }
}