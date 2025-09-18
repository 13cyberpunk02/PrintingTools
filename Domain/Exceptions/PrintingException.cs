namespace PrintingTools.Domain.Exceptions;

public class PrintingException : DomainException
{
    public PrintingException(string message) : base(message) { }
    public PrintingException(string message, Exception innerException) 
        : base(message, innerException) { }
}