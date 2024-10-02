using Microsoft.Extensions.Options;
using UsageCheckerService.Models;
using UsageCheckerService.Services;
using UsageCheckerService.Utils;

namespace UsageCheckerService;

public class Worker(
    ILogger<Worker> logger,
    UsageChecker usageChecker,
    IOptions<UsageCheckerOptions> options,
    EmailService emailService
) : BackgroundService
{
    private readonly PeriodicTimer _reportTimer = new(TimeSpan.FromSeconds(options.Value.ReportPeriodicity));
    private readonly PeriodicTimer _checkTimer = new(TimeSpan.FromSeconds(options.Value.CheckPeriodicity));

    private readonly ProcessesStack _processesStack = new(10);
    private readonly ReportPrinter _reportPrinter = new();

    private readonly object _lock = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var checkTask = HandleCheckTimer(stoppingToken);
            var reportTask = HandleReportTimer(stoppingToken);

            await Task.WhenAll(checkTask, reportTask);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred");
            throw;
        }
    }

    private async Task HandleCheckTimer(CancellationToken stoppingToken)
    {
        while (await _checkTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            var cpuUsage = usageChecker.GetCpuUsage();
            var ramUsage = usageChecker.GetMemoryUsage();
            var process = new ProcessInfo(cpuUsage, ramUsage, DateTime.Now)
            {
                UsedMemoryMeasure = "%"
            };
            lock (_lock)
            {
                _processesStack.Push(process);
            }
        }
    }

    private async Task HandleReportTimer(CancellationToken stoppingToken)
    {
        while (await _reportTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            var cpuUsage = usageChecker.GetCpuUsage();
            var ramUsage = usageChecker.GetMemoryUsage();
            var total = new ProcessInfo("Total", cpuUsage, ramUsage, DateTime.Now)
            {
                UsedMemoryMeasure = "%"
            };
            
            ProcessInfo[] history;
            lock (_lock)
            {
                _processesStack.Push(total);
                history = _processesStack.GetProcesses();
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation($"CPU: {cpuUsage}% RAM: {ramUsage}%");
            }

            if (history.Count(x => x.UsedProcessor > options.Value.CpuThreshold) >= options.Value.ThresholdHits
                || history.Count(x => x.UsedMemory > options.Value.RamThreshold) >= options.Value.ThresholdHits)
            {
                logger.LogWarning("High load detected");

                if (!emailService.IsEmailEnabled) continue;
                
                _reportPrinter.SetStateHistory(history);
                _reportPrinter.SetCurrentState(total);
                _reportPrinter.SetTopProcesses(usageChecker.GetTop5Processes());
                _reportPrinter.SetIISProcesses(usageChecker.GetWebIISProcesses(out var logFiles));
                var body = _reportPrinter.Print();

                var response = emailService.SendEmail(body, logFiles);
                if (response is not { IsSuccessful: true })
                {
                    logger.LogError("Failed to send email");
                }
            }
        }
    }
}