using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Exceptions;

public class InvalidPrintJobStateException : DomainException
{
    public PrintStatus CurrentStatus { get; }
    public string AttemptedOperation { get; }
    
    public InvalidPrintJobStateException(PrintStatus currentStatus, string attemptedOperation) 
        : base($"Невозможно выполнить операцию '{attemptedOperation}' для задания в статусе '{currentStatus}'")
    {
        CurrentStatus = currentStatus;
        AttemptedOperation = attemptedOperation;
    }
}