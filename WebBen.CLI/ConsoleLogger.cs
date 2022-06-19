using WebBen.Core.Logging;

namespace WebBen.CLI;

internal class ConsoleLogger : ILogger
{
    private readonly TextWriter _writer;

    public ConsoleLogger(TextWriter writer)
    {
        _writer = writer;
    }

    public void Info(string message)
    {
        WriteOut(nameof(Info), message);
    }

    public void Debug(string message)
    {
        if (Verbose)
            WriteOut(nameof(Debug), message);
    }

    public void Error(string message)
    {
        WriteOut(nameof(Error), message);
    }

    private void WriteOut(string level, string message)
    {
        _writer.WriteLine($"{level}: {message}");
    }

    public bool Verbose { get; set; }
}