using Microsoft.Extensions.Options;
using UsageCheckerService.Services;

namespace UsageCheckerService;

public class Worker(
    ILogger<Worker> logger, 
    UsageChecker usageChecker, 
    IOptions<UsageCheckerOptions> options, 
    EmailService emailService
    ) : BackgroundService
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(options.Value.Periodicity));
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            var cpuUsage = usageChecker.GetCpuUsage();
            var ramUsage = usageChecker.GetMemoryUsage();
            var body = $"CPU: {cpuUsage}% RAM: {ramUsage}%";
            
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(body);
            }
            
            if (cpuUsage > options.Value.CpuThreshold || ramUsage > options.Value.RamThreshold)
            {
                logger.LogWarning("High load detected");
                
                if (emailService.IsEmailEnabled)
                {
                    var response = emailService.SendEmail(body);
                    if (response is not { IsSuccessful: true })
                    {
                        logger.LogError("Failed to send email");
                    }
                }
            }
        }
    }
}