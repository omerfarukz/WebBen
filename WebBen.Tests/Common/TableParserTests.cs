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
        Assert.Throws<ArgumentException>(() => { values.ToStringTable(new string[] { }, f => string.Empty); });
    }

    [Test]
    public void Zero_Row_Returns_Should_Return_Splitter_Only()
    {
        var values = Array.Empty<int>();
        var actual = values.ToStringTable(new[] {"A"}, f => string.Empty);
        Assert.AreEqual("┌─┐\n│A│\n├─┤\n└─┘\n", actual);
    }
    
    [Test]
    public void Null_Values_Should_Pass()
    {
        var values = Array.Empty<int>();
        var actual = values.ToStringTable(new[] {"A"}, f => null);
        Assert.AreEqual("┌─┐\n│A│\n├─┤\n└─┘\n", actual);
    }
    
    [Test]
    public void Null_Header_Should_Pass()
    {
        var values = Array.Empty<int>();
        var actual = values.ToStringTable(new[] {null as string}, f => string.Empty);
        Assert.AreEqual("┌┐\n││\n├┤\n└┘\n", actual);
    }
    
    [Test]
    public void OrdinaryData_Should_Pass()
    {
        var values = Enumerable.Range(0, 3);
        var actual = values.ToStringTable(new[] {"Number"}, f => f);
        Assert.AreEqual("┌──────┐\n│Number│\n├──────┤\n│0     │\n│1     │\n│2     │\n└──────┘\n", actual);
    }
}