using Microsoft.Extensions.Options;
using UsageCheckerService.Models;
using UsageCheckerService.Services;
using UsageCheckerService.Utils;

namespace UsageCheckerService;

public class Worker(
    ILogger<Worker> logger,
    UsageChecker usageChecker,
    ProcessAvailabilityChecker processAvailabilityChecker,
    IOptions<UsageCheckerOptions> options,
    EmailService emailService
) : BackgroundService
{
    private readonly PeriodicTimer _reportTimer = new(TimeSpan.FromSeconds(options.Value.ReportPeriodicity));
    private readonly PeriodicTimer _checkTimer = new(TimeSpan.FromSeconds(options.Value.CheckPeriodicity));
    private readonly PeriodicTimer _processCheckTimer = new(TimeSpan.FromSeconds(processAvailabilityChecker.PeriodicityCheck));

    private readonly ProcessesStack _processesStack = new(10);
    private readonly ReportPrinter _reportPrinter = new();

    private readonly object _lock = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var tasksToWait = new List<Task>();
            if (options.Value.IsEnabled)
            {
                var checkTask = HandleCheckTimer(stoppingToken);
                tasksToWait.Add(checkTask);
                var reportTask = HandleReportTimer(stoppingToken);
                tasksToWait.Add(reportTask);
            }
            if (processAvailabilityChecker.IsEnabled)
            {
                var periodicityCheckTask = HandlePeriodicityCheckTimer(stoppingToken);
                tasksToWait.Add(periodicityCheckTask);
            }

            await Task.WhenAll(tasksToWait);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred");
            throw;
        }
    }
    
    private async Task HandlePeriodicityCheckTimer(CancellationToken stoppingToken)
    {
        while (await _processCheckTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            var processes = processAvailabilityChecker.GetProcessesStatus();
            if (processes.Any(x => !x.IsUp))
            {
                logger.LogWarning("Some processes are down: {Processes}", string.Join(", ", processes.Where(x => x.IsUp).Select(x => x.ProcessDisplayName)));
                var emailBody = ProcessAvailabilityChecker.GetProcessesStatusString(processes);
                if (emailService.IsEmailEnabled)
                {
                    var response = emailService.SendEmail(emailBody);
                    if (response is not { IsSuccessful: true })
                    {
                        logger.LogError("Failed to send email");
                    }
                }
            }
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