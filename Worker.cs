using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _logFilePath;

    public const string ServiceInternalName = "TempWorkerService";

    public Worker(ILogger<Worker> logger, IOptions<AppConfiguration> config, IServiceProvider serviceProvider)
    {
        _logger = logger;

        // Initialize utilities with service provider
        Initialize(serviceProvider);

        _logFilePath = GetConfiguredFilePath("ApplicationLog", config.Value.Directories.Logs);
        LogMessage($"Service '{ServiceInternalName}' initialized. Log file: {_logFilePath}", _logFilePath);
        LogMessage("This is a sample error message, in a temp dir", GetConfiguredFilePath("ErrorLog", config.Value.Directories.Temp));
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        string runMode = Environment.UserInteractive ? 
            "console mode (UserInteractive = true)" : 
            "Windows Service (UserInteractive = false)";
            
        LogMessage($"Service '{ServiceInternalName}' starting in {runMode}", _logFilePath);
        
        return base.StartAsync(cancellationToken);
    }
    
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        LogMessage($"Service '{ServiceInternalName}' stopping...", _logFilePath);
        
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogMessage($"Service '{ServiceInternalName}' execution started.", _logFilePath);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // ===== WORKER LOGIC GOES HERE =====
                
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                
                // ===== END OF WORKER LOGIC =====
                
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                LogMessage("Worker execution cancelled", _logFilePath);
                break;
            }
            catch (Exception ex)
            {
                LogMessage($"Error in worker execution: {ex.Message}", _logFilePath);
                _logger.LogError(ex, "Worker execution failed");
                
                // Wait before retrying to avoid rapid error loops
                await Task.Delay(10000, stoppingToken);
            }
        }
        
        LogMessage($"Service '{ServiceInternalName}' execution ended", _logFilePath);
    }
}
