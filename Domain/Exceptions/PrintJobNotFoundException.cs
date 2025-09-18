namespace PrintingTools.Domain.Exceptions;

public class PrintJobNotFoundException : DomainException
{
    public Guid JobId { get; }
    
    public PrintJobNotFoundException(Guid jobId) 
        : base($"Задание печати с ID {jobId} не найдено")
    {
        JobId = jobId;
    }
}