using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Services;

public interface IPrintingDomainService
{
    Task<bool> IsPrinterAvailable(string printerName);
    Task<string> ValidatePrintRequest(string filePath, string printerName);
    Task<Printer?> SelectBestPrinter(PrintFormat format, bool requireColor);
    Task<int> CalculateEstimatedTime(int pageCount, PrintQuality quality);
}