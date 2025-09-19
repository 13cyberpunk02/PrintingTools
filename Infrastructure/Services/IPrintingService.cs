using PrintingTools.Infrastructure.Services.Models;

namespace PrintingTools.Infrastructure.Services;

public interface IPrintingService
{
    Task<List<PrinterInfo>> GetAvailablePrintersAsync();
    Task<PrinterInfo?> GetPrinterStatusAsync(string printerName);
    Task<string> SendToPrinterAsync(PrintJobRequest request);
    Task<bool> CancelPrintJobAsync(string spoolJobId);
    Task<bool> PausePrintJobAsync(string spoolJobId);
    Task<bool> ResumePrintJobAsync(string spoolJobId);
    Task<PrintJobStatus?> GetPrintJobStatusAsync(string spoolJobId);
    Task<int> GetPageCountAsync(string filePath);
    Task<string> ConvertToPdfAsync(string filePath);
}