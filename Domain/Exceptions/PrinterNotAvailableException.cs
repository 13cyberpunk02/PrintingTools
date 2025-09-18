namespace PrintingTools.Domain.Exceptions;

public class PrinterNotAvailableException : PrintingException
{
    public string PrinterName { get; }
    
    public PrinterNotAvailableException(string printerName) 
        : base($"Принтер '{printerName}' недоступен")
    {
        PrinterName = printerName;
    }
}