namespace WebBen.Common.Configuration;

public class AnalyzeConfiguration : ConfigurationBase
{
    public int MaxTrialCount { get; set; } = 3;
    public CalculationFunction CalculationFunction { get; set; }
}