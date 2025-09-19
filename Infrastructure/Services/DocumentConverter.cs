namespace PrintingTools.Infrastructure.Services;

public class DocumentConverter
{
    private readonly ILogger<DocumentConverter> _logger;
    private readonly IPrintingService _printingService;
    
    public DocumentConverter(
        ILogger<DocumentConverter> logger,
        IPrintingService printingService)
    {
        _logger = logger;
        _printingService = printingService;
    }
    
    public async Task<string> ConvertToPdfAsync(string inputPath)
    {
        try
        {
            return await _printingService.ConvertToPdfAsync(inputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting file {File} to PDF", inputPath);
            throw;
        }
    }
}