using System;
using System.Linq;
using NUnit.Framework;
using WebBen.Core.Configuration;
using WebBen.Core.Extensions;

namespace WebBen.Tests.Common;

public class EnumerableOfDoubleExtensionsTests
{
    [Test]
    public void Timing_Returns_TimeSpan_For_Enumerable()
    {
        var enumerable = Enumerable.Range(0, 100).Select(f => TimeSpan.FromSeconds(f));
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Timing(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Timing(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(70), enumerable.Timing(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(80), enumerable.Timing(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(90), enumerable.Timing(CalculationFunction.P90));
        
        enumerable = Enumerable.Range(0, 4).Select(f => TimeSpan.FromSeconds(f));
        Assert.AreEqual(TimeSpan.FromSeconds(1.5), enumerable.Timing(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(1.5), enumerable.Timing(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Timing(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Timing(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Timing(CalculationFunction.P90));

        Assert.Throws<ArgumentOutOfRangeException>(() => enumerable.Percentile(-1));

    }
}