using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.PrintJobs;

namespace PrintingTools.Application.Services;

public interface IPrintService
{
    Task<ApiResponse<PrintJobDto>> CreatePrintJobAsync(Guid userId, CreatePrintJobDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PrintJobDto>> GetPrintJobByIdAsync(Guid jobId, Guid userId, bool isAdmin);
    Task<ApiResponse<PrintJobsListDto>> GetPrintJobsAsync(PagedRequest request, Guid? userId = null);
    Task<ApiResponse<PrintJobsListDto>> GetUserPrintJobsAsync(Guid userId, PagedRequest request);
    Task<ApiResponse<PrintJobDto>> StartPrintingAsync(Guid jobId, string printerName, CancellationToken cancellationToken = default);
    Task<ApiResponse<PrintJobDto>> CompletePrintJobAsync(Guid jobId, decimal cost, CancellationToken cancellationToken = default);
    Task<ApiResponse<PrintJobDto>> CancelPrintJobAsync(Guid jobId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ApiResponse<PrintJobDto>> SetPrintJobErrorAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default);
    Task<ApiResponse<PrintJobStatisticsDto>> GetStatisticsAsync(Guid? userId = null, DateTime? from = null, DateTime? to = null);
}