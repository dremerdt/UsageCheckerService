using System.Diagnostics;
using Microsoft.Web.Administration;
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
    
    private float GetUsage(ref PerformanceCounter counter)
    {
        Initialize();
        var i = counter.NextValue();
        if (i == 0)
        {
            Thread.Sleep(1000);
            i = counter.NextValue();
        }
        return float.Round(i, 2);
    }

    /// <summary>
    /// Get the CPU usage
    /// </summary>
    /// <returns></returns>
    public float GetCpuUsage() => GetUsage(ref _cpuCounter);

    /// <summary>
    /// Get the RAM usage
    /// </summary>
    /// <returns></returns>
    public float GetMemoryUsage() => GetUsage(ref _ramCounter);

    /// <summary>
    /// Get the Page usage
    /// </summary>
    /// <returns></returns>
    public float GetPageUsage() => GetUsage(ref _pageCounter);
    
    public ProcessInfo[] GetTop5Processes()
    {
        var processes = Process.GetProcesses()
            .OrderByDescending(p => p.WorkingSet64)
            .Take(5)
            .Select(p => new ProcessInfo
            {
                Name = p.ProcessName,
                UsedProcessor = double.Round(GetCpuUsageForProcess(p).Result, 2),
                UsedMemory = float.Round(p.WorkingSet64 / 1024f / 1024f, 2)
            })
            .ToArray();
        return processes;
    }

    public ProcessInfo[] GetWebIISProcesses()
    {
        var server = new ServerManager();
        var processes = server.WorkerProcesses
            .Select(p => new ProcessInfo
            {
                Name = p.ProcessId.ToString(),
                UsedProcessor = double.Round(GetCpuUsageForProcess(Process.GetProcessById(p.ProcessId)).Result, 2),
                UsedMemory = double.Round(GetMemoryUsageMbForProcess(Process.GetProcessById(p.ProcessId)).Result, 2)
            })
            .ToArray();
        return processes;
    }
    
    private async Task<double> GetCpuUsageForProcess(Process process)
    {
        var startTime = DateTime.UtcNow;
        var startCpuUsage = process.TotalProcessorTime;
        await Task.Delay(500);
    
        var endTime = DateTime.UtcNow;
        var endCpuUsage = Process.GetProcessById(process.Id).TotalProcessorTime;
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
        return cpuUsageTotal * 100;
    }
    
    private async Task<double> GetMemoryUsageMbForProcess(Process process)
    {
        await Task.Delay(500);
        return process.WorkingSet64 / 1024f / 1024f;
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
        _ramCounter.Dispose();
        _pageCounter.Dispose();
    }
}