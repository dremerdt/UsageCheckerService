namespace UsageCheckerService;

public class UsageCheckerOptions
{
    /// <summary>
    /// In seconds
    /// </summary>
    public int ReportPeriodicity { get; set; }
    /// <summary>
    /// In seconds
    /// </summary>
    public int CheckPeriodicity { get; set; }
    public int CpuThreshold { get; set; }
    public int RamThreshold { get; set; }
    public int PageThreshold { get; set; }
    /// <summary>
    /// How many times the threshold should be hit before the service takes action
    /// </summary>
    public int ThresholdHits { get; set; }
}