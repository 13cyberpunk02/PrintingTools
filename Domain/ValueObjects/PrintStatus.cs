namespace PrintingTools.Domain.ValueObjects;

public enum PrintStatus
{
    Pending = 1,      // Ожидает обработки
    Queued = 2,       // В очереди на принтере
    InProgress = 3,   // Печатается
    Paused = 4,       // Приостановлено
    Completed = 5,    // Завершено
    Failed = 6,       // Ошибка
    Cancelled = 7     // Отменено
}