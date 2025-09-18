namespace PrintingTools.Domain.Exceptions;

public class FileSizeExceededException : PrintingException
{
    public long FileSize { get; }
    public long MaxFileSize { get; }
    public string FileName { get; }
    
    public FileSizeExceededException(string fileName, long fileSize, long maxFileSize) 
        : base($"Файл '{fileName}' превышает максимально допустимый размер. Размер файла: {fileSize / 1024 / 1024} MB, максимум: {maxFileSize / 1024 / 1024} MB")
    {
        FileName = fileName;
        FileSize = fileSize;
        MaxFileSize = maxFileSize;
    }
}