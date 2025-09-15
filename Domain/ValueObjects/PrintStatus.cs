namespace PrintingTools.Domain.ValueObjects;

public enum PrintStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}