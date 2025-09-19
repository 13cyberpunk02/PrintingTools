namespace PrintingTools.Infrastructure.Services.Models;

public class PrinterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsOnline { get; set; }
    public int JobsInQueue { get; set; }
    public List<string> SupportedFormats { get; set; } = [];
    public string? Model { get; set; }
    public string? Location { get; set; }
}