namespace WebBen.CLI.Common.Logging;

internal interface ILogger
{
    void Info(string message);
    void Debug(string message);
    void Error(string message);
}