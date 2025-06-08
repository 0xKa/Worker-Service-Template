# Worker Service Template

A generic .NET 9 worker service template designed for creating background services and Windows Services with built-in configuration management, logging, utility class and deployment capabilities.

## Features

- **Modern .NET 9**: Built on the latest .NET framework
- **Windows Service Support**: Ready for Windows Service deployment
- **Configuration Management**: Strongly-typed configuration with [`AppConfiguration`](Models/AppConfiguration.cs)
- **Structured Logging**: Built-in logging with file and console output for debugging and deployment
- **Dependency Injection**: Full DI container support
- **Utility Functions** - Helper methods in [`Utilities`](Utilities.cs) class

## Start

### Prerequisites

- .NET 9.0 SDK or later
- VS Code or Visual Studio 2022
- Windows OS (for Windows Service features)

### 1. Clone or Initialize your own Worker Service

```bash
git clone <your-repo-url>
cd Worker-Service-Template
```

or

```bash
dotnet new worker -n Worker-Service-Template
cd Worker-Service-Template
```

## Logging

The template includes comprehensive logging:

- **Console Logging**: Development environment
- **File Logging**: Configurable via `EnableFileLogging`
- **Event Log**: Windows Service mode
- **Debug Output**: Debug builds

### Using the Utilities Class

The [`Utilities`](Utilities.cs) class provides helper methods:

```csharp
// Initialize with service provider
Utilities.Initialize(serviceProvider);

// Log messages to specific files
Utilities.LogMessage("Custom message", "custom.log");

// Get configured paths
string logDir = Utilities.GetConfiguredDirectory("LogDirectory");
string appLogPath = Utilities.GetConfiguredFilePath("ApplicationLog", "LogDirectory");
```

## Publishing as Windows Service

```powershell
dotnet publish -c Release -o ./publish
```

or

```powershell
dotnet publish -c Release -o ./publish --self-contained
```

> Make sure to run this from the project folder (where the .csproj file is located).

### Installing as a Windows Service

```powershell
sc create "YourServiceName" binPath="C:\path\to\publish\Worker-Service-Template.exe"
```

```powershell
sc description "YourServiceName" "Your service description"
```

```powershell
sc start "YourServiceName"
```

---

**Next Steps**: Customize the [`Worker`](Worker.cs) class with your specific business logic and update the configuration files to match your requirements.
