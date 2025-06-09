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