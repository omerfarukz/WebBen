using System.IO;
using System.Text;
using NUnit.Framework;

namespace WebBen.Tests;

public class TextWriterTestBase
{
    private MemoryStream _memoryStream;
    protected StreamWriter StreamWriter;

    [SetUp]
    public void Setup()
    {
        _memoryStream = new MemoryStream();
        StreamWriter = new StreamWriter(_memoryStream, Encoding.ASCII);
    }


    protected string ReadAndClearConsoleOut()
    {
        StreamWriter.Flush();
        var text = Encoding.ASCII.GetString(_memoryStream.ToArray());
        _memoryStream.SetLength(0);
        return text;
    }
}