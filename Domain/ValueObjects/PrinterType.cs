namespace PrintingTools.Domain.ValueObjects;

public enum PrinterType
{
    Laser = 1,      // Лазерный принтер
    Inkjet = 2,     // Струйный принтер
    Plotter = 3,    // Плоттер (для больших форматов)
    Thermal = 4,    // Термопринтер
    Virtual = 5     // Виртуальный (PDF) принтер
}