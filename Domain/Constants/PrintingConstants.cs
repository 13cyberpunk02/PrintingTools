using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Constants;

public static class PrintingConstants
{
    public static class Limits
    {
        public const int MaxCopies = 100;
        public const int MinCopies = 1;
        public const int MaxPriority = 10;
        public const int MinPriority = 1;
        public const int DefaultPriority = 5;
        public const long MaxFileSizeBytes = 104857600; // 100 MB
        public const int MaxFileNameLength = 255;
        public const int MaxQueueSize = 1000;
    }

    public static class Timeouts
    {
        public const int PrintJobTimeoutMinutes = 60;
        public const int ConversionTimeoutSeconds = 300;
        public const int StatusCheckIntervalSeconds = 5;
    }

    public static class Messages
    {
        public const string PrinterNotFound = "Принтер не найден";
        public const string PrinterOffline = "Принтер недоступен";
        public const string FileNotSupported = "Формат файла не поддерживается";
        public const string FileTooLarge = "Файл слишком большой";
        public const string QueueFull = "Очередь печати заполнена";
        public const string JobCancelled = "Задание отменено";
        public const string JobCompleted = "Печать завершена";
        public const string JobFailed = "Ошибка печати";
    }

    public static class PaperSizes
    {
        public static readonly Dictionary<PrintFormat, (int Width, int Height, string Name)> Dimensions = new()
        {
            { PrintFormat.A4, (210, 297, "A4") },
            { PrintFormat.A3, (297, 420, "A3") },
            { PrintFormat.A2, (420, 594, "A2") },
            { PrintFormat.A1, (594, 841, "A1") },
            { PrintFormat.A0, (841, 1189, "A0") },
            { PrintFormat.Custom, (0, 0, "Custom") }
        };

        public static string GetPaperSizeName(PrintFormat format)
        {
            return Dimensions.TryGetValue(format, out var size) ? size.Name : "Unknown";
        }

        public static (int Width, int Height) GetPaperDimensions(PrintFormat format)
        {
            return Dimensions.TryGetValue(format, out var size) ? (size.Width, size.Height) : (210, 297);
        }
    }
}