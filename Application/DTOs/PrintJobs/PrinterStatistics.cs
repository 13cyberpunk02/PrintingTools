namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrinterStatistics
{
    public string PrinterName { get; set; } = string.Empty;
    public int JobCount { get; set; }
    public int CompletedCount { get; set; }
    public long PagesCount { get; set; }
    public double SuccessRate { get; set; }
}