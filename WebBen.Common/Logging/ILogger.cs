namespace WebBen.Common.Logging;

public interface ILogger
{
    void Info(string message);
    void Debug(string message);
    void Error(string message);
}