namespace PrintingTools.Settings;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensitiveDataLogging { get; set; }
    public int CommandTimeout { get; set; } = 30;
}