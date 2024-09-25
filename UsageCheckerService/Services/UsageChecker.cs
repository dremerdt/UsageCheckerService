using System.Diagnostics;
using UsageCheckerService.Models;

namespace UsageCheckerService.Services;

public class UsageChecker(ILogger<Worker> logger) : IDisposable
{
    private bool _isInitialized;
    private PerformanceCounter _cpuCounter;
    private PerformanceCounter _ramCounter;
    private PerformanceCounter _pageCounter;

    private void Initialize()
    {
        if (_isInitialized)
            return;
        try
        {
            _cpuCounter = new PerformanceCounter
                ("Processor", "% Processor Time", "_Total", true);
            _ramCounter = new PerformanceCounter
                ("Memory", "% Committed Bytes In Use", string.Empty, true);
            _pageCounter = new PerformanceCounter
                ("Paging File", "% Usage", "_Total");
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize performance counters");
        }
    }
    
    private float GetUsage(PerformanceCounter counter)
    {
        Initialize();
        var i = counter.NextValue();
        if (i == 0)
        {
            Thread.Sleep(1000);
            i = counter.NextValue();
        }
        return i;
    }

    /// <summary>
    /// Get the CPU usage
    /// </summary>
    /// <returns></returns>
    public float GetCpuUsage() => GetUsage(_cpuCounter);

    /// <summary>
    /// Get the RAM usage
    /// </summary>
    /// <returns></returns>
    public float GetMemoryUsage() => GetUsage(_ramCounter);

    /// <summary>
    /// Get the Page usage
    /// </summary>
    /// <returns></returns>
    public float GetPageUsage() => GetUsage(_pageCounter);
    
    public ProcessInfo[] GetTop5Processes()
    {
        var processes = Process.GetProcesses()
            .OrderByDescending(p => p.WorkingSet64)
            .Take(5)
            .Select(p => new ProcessInfo
            {
                Name = p.ProcessName,
                UsedProcessorPercent = p.TotalProcessorTime.Milliseconds,
                UsedMemory = p.WorkingSet64 / 1024 / 1024
            })
            .ToArray();
        return processes;
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
        _ramCounter.Dispose();
        _pageCounter.Dispose();
    }
}