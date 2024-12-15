using UsageCheckerService;
using UsageCheckerService.Options;
using UsageCheckerService.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "UsageCheckerService";
});
builder.Services.AddOptions<EmailSettingsOptions>().Bind(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddOptions<UsageCheckerOptions>().Bind(builder.Configuration.GetSection("UsageChecker"));
builder.Services.AddOptions<IISMonitoringOptions>().Bind(builder.Configuration.GetSection("IISMonitoring"));
builder.Services.AddOptions<ProcessesAvailabilityOptions>().Bind(builder.Configuration.GetSection("ProcessesAvailability"));
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<UsageChecker>();
builder.Services.AddSingleton<ProcessAvailabilityChecker>();

builder.Logging.AddFile("Logs/log-{Date}.txt");

var host = builder.Build();
host.Run();