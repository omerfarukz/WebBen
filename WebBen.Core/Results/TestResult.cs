using WebBen.Core.Configuration;
using WebBen.Core.Extensions;

namespace WebBen.Core.Results;

public record TestResult(TestResultItem[] Items)
{
    public void BuildSummaries()
    {
        var calculationFunctions = Enum.GetValues(typeof(CalculationFunction));
        foreach (var item in Items)
        {
            item.Calculations = new Dictionary<CalculationFunction, TimeSpan>();
            foreach (CalculationFunction function in calculationFunctions)
            {
                item.Calculations.Add(function, item.Timings.Calculate(function));
            }
        }
    }
}