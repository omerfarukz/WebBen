namespace WebBen.CLI.Configuration;

internal class AnalyzeConfiguration : ConfigurationBase
{
    public int MaxTrialCount { get; set; } = 3;
    public CalculationFunction CalculationFunction { get; set; }
}