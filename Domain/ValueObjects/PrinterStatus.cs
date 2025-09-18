namespace PrintingTools.Domain.ValueObjects;

public enum  PrinterStatus
{
    Online = 1,       // Онлайн, готов к печати
    Offline = 2,      // Офлайн, недоступен
    Busy = 3,         // Занят печатью
    PaperJam = 4,     // Замятие бумаги
    OutOfPaper = 5,   // Закончилась бумага
    OutOfToner = 6,   // Закончился тонер/чернила
    Maintenance = 7,  // На обслуживании
    Error = 8         // Ошибка
}