# Full Guide to Create a Worker Service Template

## 1. Configure Application Settings

Update [`appsettings.json`](appsettings.json):

Exmaple Template:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AppConfiguration": {
    "Directories": {
      "Logs": "Logs",
      "Data": "Data",
      "Config": "Config",
      "Temp": "Temp",
      "Backups": "Backups",
      "Reports": "Reports",
      "Plugins": "Plugins"
    },
    "Files": {
      "ConfigFile": "appsettings.json",
      "ApplicationLog": "app.log",
      "ErrorLog": "errors.log",
      "DatabaseFile": "database.sqlite"
    },
    "CustomPaths": {
      "ApplicationLogPath": null,
      "ErrorLogPath": null,
      "DatabaseFilePath": "F:\\db\\db.log",
      "ConfigFilePath": null
    }
  }
}
```

## 2. Create AppConfiguration Model

Create a new file [`Models/AppConfiguration.cs`](Models/AppConfiguration.cs):

Exmple Template:

```csharp
namespace WorkerServiceTemplate.Models
{
    public class AppConfiguration
    {
        public DirectoryConfig Directories { get; set; } = new();
        public FileConfig Files { get; set; } = new();
        public CustomPathsConfig CustomPaths { get; set; } = new();
    }

    public class DirectoryConfig
    {
        public string Logs { get; set; } = "Logs";
        public string Data { get; set; } = "Data";
        public string Config { get; set; } = "Config";
        public string Temp { get; set; } = "Temp";
        public string Backups { get; set; } = "Backups";
        public string Reports { get; set; } = "Reports";
        public string Plugins { get; set; } = "Plugins";
    }

    public class FileConfig
    {
        public string ApplicationLog { get; set; } = "app.log";
        public string ErrorLog { get; set; } = "errors.log";
        public string ConfigFile { get; set; } = "appsettings.json";
        public string DatabaseFile { get; set; } = "database.sqlite";
    }

    public class CustomPathsConfig
    {
        // These paths can be used to override the default paths defined in FileConfig
        //can be set to null if not used
        public string? ApplicationLogPath { get; set; }
        public string? ErrorLogPath { get; set; }
        public string? DatabaseFilePath { get; set; }
        public string? ConfigFilePath { get; set; }
    }
}
```

## 3. Modify the `Program.cs`

Ensure the [`Program.cs`](./Program.cs) file is set up to read the configuration and register services:

```csharp
using WorkerServiceTemplate;
using WorkerServiceTemplate.Models;

var builder = Host.CreateApplicationBuilder(args);

// Register configuration
builder.Services.Configure<AppConfiguration>(
    builder.Configuration.GetSection("AppConfiguration"));

builder.Services.AddHostedService<Worker>();

// Enable Windows Service support
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "FileMonitoringService";
});

var host = builder.Build();
host.Run();
```

## 4. Customize the Worker

Modify the [`Worker`](Worker.cs) class to implement your business logic:

Exmple Template:

```csharp
using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _logFilePath;
    private readonly string _CustomLogFilePath;

    public const string ServiceInternalName = "TempWorkerService";

    public Worker(ILogger<Worker> logger, IOptions<AppConfiguration> config, IServiceProvider serviceProvider)
    {
        _logger = logger;

        // Initialize utilities with service provider
        Initialize(serviceProvider);

        _logFilePath = GetConfiguredFilePath("ApplicationLog", config.Value.Directories.Logs);
        LogMessage($"Service '{ServiceInternalName}' initialized. Log file: {_logFilePath}", _logFilePath);

        // Optionally, you can also log to a custom path if specified in the configuration
        _CustomLogFilePath = config.Value.CustomPaths.DatabaseFilePath ?? string.Empty;
        LogMessage($"Custom log file path configured: {_CustomLogFilePath}", _CustomLogFilePath);

        // LogMessage("This is a sample error message, in a temp dir", GetConfiguredFilePath("ErrorLog", config.Value.Directories.Temp));
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
```

## 5. Add `Utilities` Class

Create a new file [`Utilities.cs`](Utilities.cs) to provide helper methods for logging and configuration:

Example Template:

```csharp
using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;

namespace WorkerServiceTemplate;

internal class Utilities
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initialize utilities with service provider for configuration access
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Logs a message to the specified file with timestamp prefix
    /// </summary>
    public static void LogMessage(string message, string logFilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(logFilePath))
                return;

            string fullMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {message}";

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(logFilePath, fullMessage + Environment.NewLine);

            if (Environment.UserInteractive)
                Console.WriteLine(fullMessage);
        }
        catch (Exception ex)
        {
            // Fallback logging - don't let logging failures crash the service
            try
            {
                if (Environment.UserInteractive)
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: Failed to write to log file: {ex.Message}");
                else
                    System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: Failed to write to log file: {ex.Message}");
            }
            catch
            {
                // If even fallback logging fails, silently continue
            }
        }
    }

    /// <summary>
    /// Gets a specific directory path from configuration
    /// </summary>
    public static string GetConfiguredDirectory(string directoryKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryKey))
                return GetProjectRootDirectory();

            var config = GetConfiguration();
            if (config?.Directories == null)
                return GetProjectRootDirectory();

            var prop = typeof(DirectoryConfig).GetProperty(directoryKey);

            if (prop != null)
            {
                string? dirName = prop.GetValue(config.Directories)?.ToString();
                if (!string.IsNullOrEmpty(dirName))
                {
                    return CreateDirectoryWithinProject(dirName);
                }
            }

            // Return project root as fallback instead of throwing
            return GetProjectRootDirectory();
        }
        catch (Exception ex)
        {
            // Log the error and return a safe default
            SafeLog($"Error getting configured directory '{directoryKey}': {ex.Message}");
            return GetProjectRootDirectory();
        }
    }

    /// <summary>
    /// Gets a specific file path from configuration
    /// </summary>
    public static string GetConfiguredFilePath(string fileKey, string? directoryKey = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileKey))
                return Path.Combine(GetProjectRootDirectory(), "default.log");

            var config = GetConfiguration();
            if (config?.Files == null)
                return Path.Combine(GetProjectRootDirectory(), "default.log");

            var fileProp = typeof(FileConfig).GetProperty(fileKey);

            if (fileProp != null)
            {
                string? fileName = fileProp.GetValue(config.Files)?.ToString();
                if (!string.IsNullOrEmpty(fileName))
                {
                    if (!string.IsNullOrEmpty(directoryKey))
                    {
                        string directory = GetConfiguredDirectory(directoryKey);
                        return Path.Combine(directory, fileName);
                    }
                    else
                    {
                        return Path.Combine(GetProjectRootDirectory(), fileName);
                    }
                }
            }

            // Return default file path instead of throwing
            return Path.Combine(GetProjectRootDirectory(), "default.log");
        }
        catch (Exception ex)
        {
            // Log the error and return a safe default
            SafeLog($"Error getting configured file path '{fileKey}': {ex.Message}");
            return Path.Combine(GetProjectRootDirectory(), "default.log");
        }
    }

    /// <summary>
    /// Creates a directory inside another directory and returns the full path
    /// </summary>
    /// <param name="newDirName">Name of the new directory to create</param>
    /// <param name="baseDirName">Name or path of the base directory</param>
    /// <returns>Full path to the created directory</returns>
    public static string CreateDirectoryInside(string newDirName, string baseDirName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newDirName))
                throw new ArgumentException("New directory name cannot be null or empty", nameof(newDirName));

            if (string.IsNullOrWhiteSpace(baseDirName))
                throw new ArgumentException("Base directory name cannot be null or empty", nameof(baseDirName));

            // If baseDirName is not a full path, combine it with project root
            string baseDirectory = Path.IsPathRooted(baseDirName)
                ? baseDirName
                : Path.Combine(GetProjectRootDirectory(), baseDirName);

            // Ensure base directory exists
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
                SafeLog($"Created base directory: {baseDirectory}");
            }

            // Create the new directory inside the base directory
            string newDirectoryPath = Path.Combine(baseDirectory, newDirName);

            if (!Directory.Exists(newDirectoryPath))
            {
                Directory.CreateDirectory(newDirectoryPath);
                SafeLog($"Created directory: {newDirectoryPath}");
            }

            return newDirectoryPath;
        }
        catch (Exception ex)
        {
            SafeLog($"Error creating directory '{newDirName}' inside '{baseDirName}': {ex.Message}");
            // Return a fallback path
            return Path.Combine(GetProjectRootDirectory(), newDirName);
        }
    }


    /// <summary>
    /// Gets configuration from dependency injection
    /// </summary>
    private static AppConfiguration? GetConfiguration()
    {
        try
        {
            if (_serviceProvider == null)
                return null;

            var options = _serviceProvider.GetRequiredService<IOptions<AppConfiguration>>();
            return options?.Value;
        }
        catch (Exception ex)
        {
            SafeLog($"Error getting configuration: {ex.Message}");
            return null;
        }
    }

    private static string GetProjectRootDirectory()
    {
        try
        {
            if (Environment.UserInteractive)
                // Running in console or debugger
                return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
            else
                // Running as a Windows Service, AppContext.BaseDirectory will be the Publish/ folder
                return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\"));
        }
        catch (Exception ex)
        {
            SafeLog($"Error getting project root directory: {ex.Message}");
            // Fallback to current directory
            return AppContext.BaseDirectory;
        }
    }

    private static string CreateDirectoryWithinProject(string directoryName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                return GetProjectRootDirectory();

            string newDirectory = Path.Combine(GetProjectRootDirectory(), directoryName);

            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            return newDirectory;
        }
        catch (Exception ex)
        {
            SafeLog($"Error creating directory '{directoryName}': {ex.Message}");
            // Fallback to project root
            return GetProjectRootDirectory();
        }
    }

    /// <summary>
    /// Safe logging method that won't throw exceptions
    /// </summary>
    private static void SafeLog(string message)
    {
        try
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UTILITIES ERROR: {message}";

            if (Environment.UserInteractive)
                Console.WriteLine(logMessage);
            else
                System.Diagnostics.Debug.WriteLine(logMessage);
        }
        catch
        {
            // If even this fails, silently continue
        }
    }
}
```

## 6. Run the Application

### Development Mode

```powershell
dotnet run
```

### Production Build

```powershell
dotnet build --configuration Release
dotnet run --configuration Release
```
