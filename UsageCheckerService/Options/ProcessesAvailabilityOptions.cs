using UsageCheckerService.Models;

namespace UsageCheckerService.Options;

public class ProcessesAvailabilityOptions
{
    /// <summary>
    /// In seconds
    /// </summary>
    public int CheckPeriodicity { get; set; }
    public bool IsEnabled { get; set; }
    public ProcessStatus[] Processes { get; set; }
}