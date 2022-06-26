using WebBen.Core.Configuration;

namespace WebBen.Core.Extensions;

public static class EnumerableOfDoubleExtensions
{
    public static TimeSpan Calculate(this IEnumerable<TimeSpan>? source, CalculationFunction calculationFunction)
    {
        return calculationFunction switch
        {
            CalculationFunction.Average => source.Average(),
            CalculationFunction.StdDev => source.StdDev(),
            CalculationFunction.Median => source.Median(),
            CalculationFunction.P90 => source.Percentile(0.9d),
            CalculationFunction.P80 => source.Percentile(0.8d),
            CalculationFunction.P70 => source.Percentile(0.7d),
            _ => throw new NotSupportedException(
                $"{nameof(calculationFunction)}: {calculationFunction}")
        };
    }

    private static TimeSpan StdDev(this IEnumerable<TimeSpan>? source)
    {
        return CastDoubleAndProcess(source, doubles =>
        {
            var enumerable = doubles as double[] ?? doubles.ToArray();
            if (!enumerable.Any())
                return 0;

            var avg = enumerable.Average();
            return Math.Sqrt(enumerable.Average(v => Math.Pow(v - avg, 2)));
        });
    }

    private static TimeSpan Average(this IEnumerable<TimeSpan>? source)
    {
        return CastDoubleAndProcess(source, doubles => doubles.Average());
    }

    private static TimeSpan Percentile(this IEnumerable<TimeSpan>? source, double percent)
    {
        return CastDoubleAndProcess(source, doubles => doubles.Percentile(percent));
    }

    private static TimeSpan Median(this IEnumerable<TimeSpan>? source)
    {
        return CastDoubleAndProcess(source, doubles => doubles.Median());
    }

    private static TimeSpan CastDoubleAndProcess(this IEnumerable<TimeSpan>? source,
        Func<IEnumerable<double>, double> func)
    {
        return TimeSpan.FromMilliseconds(func(source.Select(f => f.TotalMilliseconds)));
    }

    private static double Average(this IEnumerable<double> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var num = 0d;
        var num2 = 0L;
        foreach (var num3 in source)
        {
            num += num3;
            num2 += 1;
        }

        if (num2 == 0)
            return 0d;

        return num / num2;
    }

    private static double Percentile(this IEnumerable<double> source, double percent)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (percent is < 0.0 or > 1.0)
            throw new ArgumentOutOfRangeException(nameof(percent), "percent must be between 0.0 and 1.0");

        var array = source.DefaultIfEmpty(0).OrderBy(x => x).ToArray();
        var num = (int) Math.Floor(percent * array.Length);
        return array[num];
    }

    private static double Median(this IEnumerable<double> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var array = source.OrderBy(x => x).ToArray();
        var num = array.Length;
        if (num == 0) return 0;

        if (num % 2 == 0)
            return (array[num / 2] + array[num / 2 - 1]) / 2.0;

        return array[num / 2];
    }
}