using PrintingTools.Domain.Common;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Entities;

public class PrintJob : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? PrinterId { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string FileType { get; private set; } // pdf, docx, xlsx, etc.
    public PrintFormat Format { get; private set; }
    public int Copies { get; private set; }
    public bool IsColor { get; private set; }
    public bool IsDuplex { get; private set; }
    public PrintQuality Quality { get; private set; }
    public PrintStatus Status { get; private set; }
    public int Priority { get; private set; } // 1-10, где 10 - наивысший
    public int? TotalPages { get; private set; }
    public int? PrintedPages { get; private set; }
    public DateTime? QueuedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SpoolJobId { get; private set; } // ID задания в системе печати (CUPS/Windows)
    
    // Навигационные свойства
    public User User { get; private set; }
    public Printer? Printer { get; private set; }

    protected PrintJob() { } // Для EF Core

    public PrintJob(
        Guid userId,
        string fileName,
        string filePath,
        long fileSizeBytes,
        string fileType,
        PrintFormat format,
        int copies = 1,
        bool isColor = true,
        bool isDuplex = false,
        PrintQuality quality = PrintQuality.Normal,
        int priority = 5)
    {
        UserId = userId;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileSizeBytes = fileSizeBytes;
        FileType = fileType?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(fileType));
        Format = format;
        Copies = copies > 0 ? copies : throw new ArgumentException("Количество копий должно быть больше 0");
        IsColor = isColor;
        IsDuplex = isDuplex;
        Quality = quality;
        Priority = priority >= 1 && priority <= 10 ? priority : 5;
        Status = PrintStatus.Pending;
    }

    public void AssignToPrinter(Guid printerId)
    {
        if (Status != PrintStatus.Pending)
            throw new InvalidOperationException("Можно назначить принтер только для заданий в статусе 'Ожидание'");

        PrinterId = printerId;
        Status = PrintStatus.Queued;
        QueuedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartPrinting(string spoolJobId, int totalPages)
    {
        if (Status != PrintStatus.Queued)
            throw new InvalidOperationException("Задание должно быть в очереди перед началом печати");

        SpoolJobId = spoolJobId ?? throw new ArgumentNullException(nameof(spoolJobId));
        TotalPages = totalPages > 0 ? totalPages : throw new ArgumentException("Количество страниц должно быть больше 0");
        PrintedPages = 0;
        Status = PrintStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(int printedPages)
    {
        if (Status != PrintStatus.InProgress)
            throw new InvalidOperationException("Задание должно быть в процессе печати");

        if (printedPages < 0)
            throw new ArgumentException("Количество напечатанных страниц не может быть отрицательным");

        PrintedPages = printedPages;
        UpdatedAt = DateTime.UtcNow;

        if (TotalPages.HasValue && PrintedPages >= TotalPages.Value)
        {
            Complete();
        }
    }

    public void Complete()
    {
        if (Status != PrintStatus.InProgress)
            throw new InvalidOperationException("Можно завершить только задание в процессе печати");

        Status = PrintStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != PrintStatus.InProgress)
            throw new InvalidOperationException("Можно приостановить только активное задание");

        Status = PrintStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resume()
    {
        if (Status != PrintStatus.Paused)
            throw new InvalidOperationException("Можно возобновить только приостановленное задание");

        Status = PrintStatus.InProgress;
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
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Сообщение об ошибке не может быть пустым");

        Status = PrintStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    // Бизнес-правила
    public bool CanBeCancelled() => Status != PrintStatus.Completed && Status != PrintStatus.Cancelled;
    
    public bool CanBePaused() => Status == PrintStatus.InProgress;
    
    public bool CanBeResumed() => Status == PrintStatus.Paused;
    
    public bool IsActive() => Status == PrintStatus.Pending || Status == PrintStatus.Queued || 
                              Status == PrintStatus.InProgress || Status == PrintStatus.Paused;
    
    public int GetProgressPercentage()
    {
        if (TotalPages is null or 0)
            return 0;
        
        if (!PrintedPages.HasValue)
            return 0;
        
        return (int)((PrintedPages.Value * 100.0) / TotalPages.Value);
    }
}