using System;
using System.Collections.Generic;
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
        IEnumerable<TimeSpan> enumerable = Enumerable.Range(0, 100).Select(f => TimeSpan.FromSeconds(f));
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(70), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(80), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(90), enumerable.Calculate(CalculationFunction.P90));
        
        enumerable = Enumerable.Range(1, 3).Select(f => TimeSpan.FromSeconds(f));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P90));

        Assert.Throws<ArgumentOutOfRangeException>(() => enumerable.Percentile(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => enumerable.Percentile(2));

        enumerable = new TimeSpan[] { };
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P90));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P70));

        Assert.Throws<ArgumentNullException>(() => { EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.Average); });
        Assert.Throws<ArgumentNullException>(() => { EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.Median); });
        Assert.Throws<ArgumentNullException>(() => { EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P70); });
        Assert.Throws<ArgumentNullException>(() => { EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P80); });
        Assert.Throws<ArgumentNullException>(() => { EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P90); });
    }
}