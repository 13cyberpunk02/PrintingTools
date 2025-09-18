namespace PrintingTools.Domain.Exceptions;

public class FileNotSupportedException : PrintingException
{
    public string FileType { get; }
    
    public FileNotSupportedException(string fileType) 
        : base($"Тип файла '{fileType}' не поддерживается")
    {
        FileType = fileType;
    }
}