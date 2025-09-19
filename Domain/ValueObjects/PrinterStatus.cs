namespace PrintingTools.Domain.ValueObjects;

public enum PrinterStatus
{
    Online = 1,
    Offline = 2,
    Busy = 3,
    PaperJam = 4,
    OutOfPaper = 5,
    OutOfToner = 6,
    Maintenance = 7,
    Error = 8
}