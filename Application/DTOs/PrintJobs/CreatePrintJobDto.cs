namespace PrintingTools.Application.DTOs.PrintJobs;

public class CreatePrintJobDto
{
    public IFormFile File { get; set; } = null!;
    public string Format { get; set; } = "A4";
    public int Copies { get; set; } = 1;
    public bool IsColor { get; set; } = true;
    public bool IsDuplex { get; set; } = false;
    public string Quality { get; set; } = "Normal";
    public int Priority { get; set; } = 5;
    public string? PrinterName { get; set; }
}