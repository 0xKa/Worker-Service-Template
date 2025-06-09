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