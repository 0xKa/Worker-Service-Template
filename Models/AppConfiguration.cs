namespace WorkerServiceTemplate.Models
{
    public class AppConfiguration
    {
        public DirectoryConfig Directories { get; set; } = new();
        public FileConfig Files { get; set; } = new();
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

}