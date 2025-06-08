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

```powershell
git clone <your-repo-url>
cd Worker-Service-Template
```

or

```powershell
dotnet new worker -n Worker-Service-Template
cd Worker-Service-Template
```

### 2. Install Required Packages

```powershell
dotnet add package Microsoft.Extensions.Hosting.WindowsServices
```

### 3. Configure the Worker Service

Follow this deatiled guide to set up the worker service template:

> [Worker Service Configuration Guide](./FullGuide.md)

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

#### Setting service description:

```powershell
sc description "YourServiceName" "Your service description"
```

---

### Miscillaneous Commands

#### 🔍 1. Check Service Status

```powershell
sc query MyWorkerService
```

#### ▶️ 2. Start the Service

```powershell
sc start MyWorkerService
```

Or via `services.msc`.

---

#### ❌ 3. Stop and Delete the Service

```powershell
sc stop MyWorkerService
sc delete MyWorkerService
```

---

#### 🔁 4. Reinstall After Code Changes

1. Stop and delete the service:s

2. Re-publish:

   ```powershell
   dotnet publish -c Release -o ./Publish
   ```

   > Make sure to run this from the project folder (where the `.csproj` file is located).

3. Re-create the service:

   ```powershell
   sc create MyWorkerService binPath= "C:\Path\To\Publish\MyWorkerService.exe"
   ```

4. Start it:

   ```powershell
   sc start MyWorkerService
   ```

---

**Next Steps**: Customize the [`Worker`](Worker.cs) class with your specific business logic and update the configuration files to match your requirements.
