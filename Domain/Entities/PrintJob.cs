using PrintingTools.Domain.Common;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Entities;

public class PrintJob : BaseEntity
{
    public Guid UserId { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public PrintFormat Format { get; private set; }
    public int Copies { get; private set; }
    public PrintStatus Status { get; private set; }
    public string? PrinterName { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public decimal? Cost { get; private set; }
    
    // Навигационные свойства
    public User User { get; private set; }

    protected PrintJob() { } // Для EF Core

    public PrintJob(
        Guid userId,
        string fileName,
        string filePath,
        long fileSizeBytes,
        PrintFormat format,
        int copies = 1)
    {
        UserId = userId;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileSizeBytes = fileSizeBytes;
        Format = format;
        Copies = copies > 0 ? copies : throw new ArgumentException("Количество копий должно быть больше 0");
        Status = PrintStatus.Pending;
    }

    public void StartPrinting(string printerName)
    {
        if (Status != PrintStatus.Pending)
            throw new InvalidOperationException("Можно начать печать только для заданий в статусе 'Ожидание'");
        
        PrinterName = printerName;
        Status = PrintStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(decimal cost)
    {
        if (Status != PrintStatus.InProgress)
            throw new InvalidOperationException("Можно завершить только задания в процессе печати");
        
        Status = PrintStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Cost = cost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PrintStatus.Completed)
            throw new InvalidOperationException("Нельзя отменить завершенное задание");
        
        Status = PrintStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetError(string errorMessage)
    {
        Status = PrintStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }
}