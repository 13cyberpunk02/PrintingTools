using System.Linq.Expressions;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Specifications;

public static class PrinterSpecifications
{
    public static Expression<Func<Printer, bool>> IsOnline()
    {
        return printer => printer.Status == PrinterStatus.Online;
    }

    public static Expression<Func<Printer, bool>> IsAvailable()
    {
        return printer => printer.Status == PrinterStatus.Online && 
                          !printer.IsDeleted;
    }

    public static Expression<Func<Printer, bool>> ByType(PrinterType type)
    {
        return printer => printer.Type == type;
    }

    public static Expression<Func<Printer, bool>> SupportsColor()
    {
        return printer => printer.IsColorSupported;
    }

    public static Expression<Func<Printer, bool>> CanPrintFormat(PrintFormat format)
    {
        return printer => printer.CanPrint(format);
    }

    public static Expression<Func<Printer, bool>> IsDefault()
    {
        return printer => printer.IsDefault;
    }

    public static Expression<Func<Printer, bool>> NeedsMaintenance()
    {
        return printer => printer.NeedsMaintenance(10000);
    }

    public static Expression<Func<Printer, bool>> HasError()
    {
        return printer => printer.Status == PrinterStatus.Error ||
                          printer.Status == PrinterStatus.PaperJam ||
                          printer.Status == PrinterStatus.OutOfPaper ||
                          printer.Status == PrinterStatus.OutOfToner;
    }

    public static Expression<Func<Printer, bool>> ByLocation(string location)
    {
        return printer => printer.Location.Contains(location, StringComparison.OrdinalIgnoreCase);
    }
}