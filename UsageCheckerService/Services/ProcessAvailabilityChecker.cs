using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using UsageCheckerService.Models;
using UsageCheckerService.Options;

namespace UsageCheckerService.Services;

public class ProcessAvailabilityChecker(IOptions<ProcessesAvailabilityOptions> configuration)
{
    private readonly ProcessesAvailabilityOptions _processesToCheck = configuration.Value;

    public int PeriodicityCheck => configuration.Value.CheckPeriodicity;
    public bool IsEnabled => configuration.Value.IsEnabled;

    public ProcessStatus[] GetProcessesStatus()
    {
        var processes = new List<ProcessStatus>();
        foreach (var process in _processesToCheck.Processes)
        {
            var processStatus = new ProcessStatus
            {
                ProcessName = process.ProcessName,
                ProcessDisplayName = process.ProcessDisplayName,
                IsUp = Process.GetProcessesByName(process.ProcessName).Length != 0
            };
            processes.Add(processStatus);
        }
        return processes.ToArray();
    }
    
    public static string GetProcessesStatusString(ProcessStatus[] processes)
    {
        if (processes == null || processes.Length == 0)
        {
            return string.Empty;
        }
        
        var sb = new StringBuilder();
        sb.Append("<br/><br/><b>Processes status:</b><br/>");
        foreach (var process in processes)
        {
            sb.AppendLine($" - {process.ProcessDisplayName} is {(process.IsUp ? "up" : "down")}<br/>");
        }
        return sb.ToString();
    }
}