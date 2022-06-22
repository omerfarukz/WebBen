using System;
using System.Linq;
using NUnit.Framework;
using WebBen.Core.Extensions;

namespace WebBen.Tests.Common;

public class TableParserTests
{
    [Test]
    public void Different_Size_Of_Headers_And_Values_Should_Throws()
    {
        var values = Array.Empty<int>();
        Assert.Throws<ArgumentException>(() =>
        {
            values.ToStringTable(new string []{}, f => string.Empty);
        });
    }
    
    [Test]
    public void Zero_Row_Returns_Should_Return_Splitter_Only()
    {
        var values = Array.Empty<int>();
        var actual = values.ToStringTable(new string []{ "A"}, f => string.Empty);
        Assert.AreNotEqual(" | A | \r\n|---| \r\n", actual);
    }
}