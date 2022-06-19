using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using WebBen.Core.Configuration.Source;

namespace WebBen.Tests.Common;

public class FileConfigurationSourceTests
{
    private string _content;
    private string _filePath;

    [SetUp]
    public void Setup()
    {
        _content = Guid.NewGuid().ToString();
        _filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        File.WriteAllText(_filePath, _content);
    }

    [Test]
    public async Task FileConfigurationSource_Should_Return_Correct_Content()
    {
        var source = new FileConfigurationSource(_filePath);
        Assert.NotNull(source);
        Assert.AreEqual(_content, await source.GetContent());
    }

    [Test]
    public void FileConfigurationSource_Should_Throw_Exception_When_File_Does_Not_Exist()
    {
        var source = new FileConfigurationSource(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        Assert.NotNull(source);
        Assert.ThrowsAsync<FileNotFoundException>(() => source.GetContent());
    }

    [TearDown]
    public void TearDown()
    {
        File.Delete(_filePath);
    }
}