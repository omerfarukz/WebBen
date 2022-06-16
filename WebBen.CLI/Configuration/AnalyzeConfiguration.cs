namespace WebBen.CLI.Configuration;

internal class AnalyzeConfiguration : ConfigurationBase
{
    public int MaxTrialCount { get; set; } = 3;
    public CalculationFunciton CalculationFunction { get; set; }
}

public enum CalculationFunciton
{
    Average,
    P90,
    P80,
    P70,
    Median
}