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
        var enumerable = Enumerable.Range(0, 100).Select(f => TimeSpan.FromSeconds(f)).ToArray();
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(49.5), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(70), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(80), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(90), enumerable.Calculate(CalculationFunction.P90));
        Assert.AreEqual(288660700, enumerable.Calculate(CalculationFunction.StdDev).Ticks);

        enumerable = Enumerable.Range(1, 2).Select(f => TimeSpan.FromSeconds(f)).ToArray();
        Assert.AreEqual(TimeSpan.FromSeconds(1.5), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(1.5), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.P90));
        Assert.AreEqual(5000000, enumerable.Calculate(CalculationFunction.StdDev).Ticks);

        enumerable = Enumerable.Range(1, 3).Select(f => TimeSpan.FromSeconds(f)).ToArray();
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(2), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(3), enumerable.Calculate(CalculationFunction.P90));
        Assert.AreEqual(8164965, enumerable.Calculate(CalculationFunction.StdDev).Ticks);

        enumerable = new TimeSpan[] { };
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.Average));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.Median));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P90));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P80));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.P70));
        Assert.AreEqual(TimeSpan.FromSeconds(0), enumerable.Calculate(CalculationFunction.StdDev));

        Assert.IsFalse(Enum.IsDefined(typeof(CalculationFunction), int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var calculationFunction = (CalculationFunction)int.MaxValue;
            Enumerable.Empty<TimeSpan>().Calculate(calculationFunction);
        });
        
        Assert.Throws<ArgumentNullException>(() =>
        {
            EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.Average);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.Median);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P70);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P80);
        });
        Assert.Throws<ArgumentNullException>(() =>
        {
            EnumerableOfDoubleExtensions.Calculate(null!, CalculationFunction.P90);
        });
        
    }
}