namespace PrintingTools.Domain.Exceptions;

public class InvalidFileFormatException : PrintingException
{
    public string FileName { get; }
    public string ExpectedFormat { get; }
    public string ActualFormat { get; }
    
    public InvalidFileFormatException(string fileName, string actualFormat, string expectedFormat) 
        : base($"Неверный формат файла '{fileName}'. Ожидается: {expectedFormat}, получен: {actualFormat}")
    {
        FileName = fileName;
        ExpectedFormat = expectedFormat;
        ActualFormat = actualFormat;
    }
}