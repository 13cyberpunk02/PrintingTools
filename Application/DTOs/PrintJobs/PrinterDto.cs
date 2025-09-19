namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrinterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string NetworkPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsColorSupported { get; set; }
    public bool IsOnline { get; set; }
    public int? JobsInQueue { get; set; }
    public List<string> SupportedFormats { get; set; } = new();
    public int MaxPaperWidth { get; set; }
    public int MaxPaperHeight { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public int PagesPrinted { get; set; }
    public bool NeedsMaintenance { get; set; }
}