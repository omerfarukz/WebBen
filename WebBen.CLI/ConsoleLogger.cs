using WebBen.Common.Logging;

namespace WebBen.CLI;

internal class ConsoleLogger : ILogger
{
    public void Info(string message)
    {
        Console.WriteLine(message);
    }

    public void Debug(string message)
    {
        if (Verbose)
            Console.WriteLine(message);
    }

    public void Error(string message)
    {
        if(!Verbose)
            return;
        
        lock (Console.Out)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = foregroundColor;
        }
    }

    public bool Verbose { get; set; }
}