namespace WebBen.Core.Configuration;

public record AnalyzeConfiguration : ConfigurationBase
{
    public int MaxTrialCount { get; set; } = 3;
    public CalculationFunction CalculationFunction { get; set; } = CalculationFunction.Average;
}