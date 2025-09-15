namespace PrintingTools.Application.DTOs.Common;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}