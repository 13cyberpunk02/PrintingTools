using PrintingTools.Domain.Common;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Entities;

public class Printer : BaseEntity
{
     private readonly List<string> _supportedFormats = new();

    public string Name { get; private set; }
    public string Model { get; private set; }
    public string Location { get; private set; }
    public PrinterType Type { get; private set; }
    public string NetworkPath { get; private set; } // IP адрес или сетевой путь (например: 192.168.1.100 или \\server\printer)
    public PrinterStatus Status { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsColorSupported { get; private set; }
    public int MaxPaperWidth { get; private set; } // в мм
    public int MaxPaperHeight { get; private set; } // в мм
    public DateTime? LastMaintenanceDate { get; private set; }
    public int PagesPrinted { get; private set; }
    public int? CurrentJobsInQueue { get; private set; }
    
    // Поддерживаемые форматы файлов
    public IReadOnlyCollection<string> SupportedFormats => _supportedFormats.AsReadOnly();

    protected Printer() { } // Для EF Core

    public Printer(
        string name,
        string model,
        string location,
        PrinterType type,
        string networkPath,
        bool isColorSupported = true,
        int maxPaperWidth = 841, // A0 width по умолчанию
        int maxPaperHeight = 1189) // A0 height по умолчанию
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Type = type;
        NetworkPath = networkPath ?? throw new ArgumentNullException(nameof(networkPath));
        Status = PrinterStatus.Online;
        IsColorSupported = isColorSupported;
        MaxPaperWidth = maxPaperWidth > 0 ? maxPaperWidth : throw new ArgumentException("Ширина должна быть больше 0");
        MaxPaperHeight = maxPaperHeight > 0 ? maxPaperHeight : throw new ArgumentException("Высота должна быть больше 0");
        PagesPrinted = 0;
        
        // Добавляем стандартные поддерживаемые форматы
        InitializeSupportedFormats();
    }

    private void InitializeSupportedFormats()
    {
        _supportedFormats.Add("pdf");
        _supportedFormats.Add("ps");
        
        if (Type == PrinterType.Plotter)
        {
            _supportedFormats.Add("plt");
            _supportedFormats.Add("hpgl");
        }
        
        // Все принтеры поддерживают изображения
        _supportedFormats.Add("jpg");
        _supportedFormats.Add("jpeg");
        _supportedFormats.Add("png");
        _supportedFormats.Add("bmp");
        _supportedFormats.Add("tiff");
    }

    public void SetStatus(PrinterStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementPageCount(int pages)
    {
        if (pages <= 0)
            throw new ArgumentException("Количество страниц должно быть больше 0");

        PagesPrinted += pages;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateJobsInQueue(int jobsCount)
    {
        CurrentJobsInQueue = jobsCount >= 0 ? jobsCount : 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordMaintenance()
    {
        LastMaintenanceDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddSupportedFormat(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Формат не может быть пустым");

        var normalizedFormat = format.ToLowerInvariant().TrimStart('.');
        if (!_supportedFormats.Contains(normalizedFormat))
        {
            _supportedFormats.Add(normalizedFormat);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Бизнес-правила
    public bool CanPrint(PrintFormat format)
    {
        if (Status != PrinterStatus.Online)
            return false;

        // Проверка размера бумаги
        var (width, height) = GetPaperSize(format);
        return width <= MaxPaperWidth && height <= MaxPaperHeight;
    }

    public bool CanPrintFile(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            return false;

        var normalizedType = fileType.ToLowerInvariant().TrimStart('.');
        return _supportedFormats.Contains(normalizedType);
    }

    public bool IsAvailable() => Status == PrinterStatus.Online;

    public bool NeedsMaintenance(int pageThreshold = 10000)
    {
        // Проверяем, нужно ли обслуживание
        if (LastMaintenanceDate == null)
            return PagesPrinted > pageThreshold;

        var daysSinceLastMaintenance = (DateTime.UtcNow - LastMaintenanceDate.Value).Days;
        return daysSinceLastMaintenance > 90 || PagesPrinted > pageThreshold;
    }

    private (int width, int height) GetPaperSize(PrintFormat format)
    {
        return format switch
        {
            PrintFormat.A4 => (210, 297),
            PrintFormat.A3 => (297, 420),
            PrintFormat.A2 => (420, 594),
            PrintFormat.A1 => (594, 841),
            PrintFormat.A0 => (841, 1189),
            PrintFormat.Custom => (MaxPaperWidth, MaxPaperHeight),
            _ => (210, 297) // По умолчанию A4
        };
    }
}