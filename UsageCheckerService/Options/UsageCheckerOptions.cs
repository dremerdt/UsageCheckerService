namespace UsageCheckerService;

public class UsageCheckerOptions
{
    public int Periodicity { get; set; }
    public int CpuThreshold { get; set; }
    public int RamThreshold { get; set; }
    public int PageThreshold { get; set; }
}