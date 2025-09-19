using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Services.Models;

public class PrintJobRequest
{
    public string PrinterName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int Copies { get; set; } = 1;
    public bool IsColor { get; set; } = true;
    public bool IsDuplex { get; set; } = false;
    public PrintQuality Quality { get; set; } = PrintQuality.Normal;
    public string? PageRange { get; set; } 
}