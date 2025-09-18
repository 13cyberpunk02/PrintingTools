namespace PrintingTools.Domain.Exceptions;

public class QueueFullException : PrintingException
{
    public int MaxQueueSize { get; }
    public int CurrentSize { get; }
    
    public QueueFullException(int maxQueueSize, int currentSize) 
        : base($"Очередь печати заполнена. Максимум: {maxQueueSize}, текущий размер: {currentSize}")
    {
        MaxQueueSize = maxQueueSize;
        CurrentSize = currentSize;
    }
}