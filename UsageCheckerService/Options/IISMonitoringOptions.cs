namespace UsageCheckerService;

public class IISMonitoringOptions
{
    public int CpuThreshold { get; set; }
    
    public LogLocationForAppPool[] LogLocationForAppPools { get; set; }
}

public class LogLocationForAppPool
{
    public string AppPoolName { get; set; }
    public string LogLocation { get; set; }
}