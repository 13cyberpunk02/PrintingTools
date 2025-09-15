namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrintJobsListDto
{
    public List<PrintJobDto> PrintJobs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
    