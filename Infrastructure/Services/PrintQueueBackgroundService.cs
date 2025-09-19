using System.Threading.Channels;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Repositories;
using PrintingTools.Infrastructure.Services.Models;

namespace PrintingTools.Infrastructure.Services;

public class PrintQueueBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPrintingService _printingService;
    private readonly ILogger<PrintQueueBackgroundService> _logger;
    private readonly Channel<Guid> _printQueue;
    private readonly Dictionary<Guid, JobInfo> _activeJobs;
    private readonly Timer _statusCheckTimer;

    public PrintQueueBackgroundService(
        IServiceProvider serviceProvider,
        IPrintingService printingService,
        ILogger<PrintQueueBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _printingService = printingService;
        _logger = logger;
        _printQueue = Channel.CreateUnbounded<Guid>();
        _activeJobs = new Dictionary<Guid, JobInfo>();
        
        _statusCheckTimer = new Timer(CheckActiveJobs, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    public async Task EnqueuePrintJobAsync(Guid printJobId)
    {
        await _printQueue.Writer.WriteAsync(printJobId);
        _activeJobs[printJobId] = new JobInfo { Status = "Pending", EnqueuedAt = DateTime.UtcNow };
        _logger.LogInformation("Задание c Id: {JobId} добавлено в очередь печати", printJobId);
    }

    public async Task<bool> CancelPrintJobAsync(Guid printJobId)
    {
        if (!_activeJobs.TryGetValue(printJobId, out var jobInfo)) return false;
        if (!string.IsNullOrEmpty(jobInfo.SpoolJobId))
        {
            var result = await _printingService.CancelPrintJobAsync(jobInfo.SpoolJobId);
            if (result)
            {
                _activeJobs.Remove(printJobId);
            }
            return result;
        }
            
        _activeJobs.Remove(printJobId);
        return true;
    }

    public async Task<bool> PausePrintJobAsync(Guid printJobId)
    {
        if (!_activeJobs.TryGetValue(printJobId, out var jobInfo)) return false;
        if (string.IsNullOrEmpty(jobInfo.SpoolJobId)) return false;
        var result = await _printingService.PausePrintJobAsync(jobInfo.SpoolJobId);
        if (result)
        {
            jobInfo.Status = "Paused";
        }
        return result;
    }

    public async Task<bool> ResumePrintJobAsync(Guid printJobId)
    {
        if (!_activeJobs.TryGetValue(printJobId, out var jobInfo)) return false;
        if (string.IsNullOrEmpty(jobInfo.SpoolJobId)) return false;
        var result = await _printingService.ResumePrintJobAsync(jobInfo.SpoolJobId);
        if (result)
        {
            jobInfo.Status = "Processing";
        }
        return result;
    }

    public Task<QueueStatusInfo> GetQueueStatusAsync()
    {
        var status = new QueueStatusInfo
        {
            TotalJobs = _activeJobs.Count,
            PendingJobs = _activeJobs.Count(j => j.Value.Status == "Pending"),
            ProcessingJobs = _activeJobs.Count(j => j.Value.Status == "Processing"),
            Jobs = _activeJobs.Select((kvp, index) => new JobStatusInfo
            {
                JobId = kvp.Key,
                Position = index + 1,
                Status = kvp.Value.Status,
                EnqueuedAt = kvp.Value.EnqueuedAt
            }).ToList()
        };
        
        return Task.FromResult(status);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба очереди печати запущено");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var printJobId = await _printQueue.Reader.ReadAsync(stoppingToken);
                await ProcessPrintJob(printJobId);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в процессе очереди печати");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Служба очереди печати остановлено");
    }

    private async Task ProcessPrintJob(Guid printJobId)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var printJob = await unitOfWork.PrintJobs.GetByIdAsync(printJobId);
            if (printJob == null)
            {
                _logger.LogWarning("Задание на печать с Id: {JobId} не найдено", printJobId);
                _activeJobs.Remove(printJobId);
                return;
            }

            if (printJob.Status != PrintStatus.Pending && printJob.Status != PrintStatus.Queued)
            {
                _logger.LogWarning("Задание с Id: {JobId} имеет статус {Status}, пропускаю", 
                    printJobId, printJob.Status);
                _activeJobs.Remove(printJobId);
                return;
            }

            var printer = await SelectPrinterAsync(printJob, unitOfWork);
            if (printer == null)
            {
                printJob.SetError("No available printer for this format");
                await unitOfWork.CompleteAsync();
                _activeJobs.Remove(printJobId);
                return;
            }

            printJob.AssignToPrinter(printer.Id);
            await unitOfWork.CompleteAsync();

            var fileToPrint = await PrepareFileAsync(printJob.FilePath, printJob.FileType);
            
            var pageCount = await _printingService.GetPageCountAsync(fileToPrint);

            var request = new PrintJobRequest
            {
                PrinterName = printer.NetworkPath,
                FilePath = fileToPrint,
                Copies = printJob.Copies,
                IsColor = printJob.IsColor,
                IsDuplex = printJob.IsDuplex,
                Quality = printJob.Quality
            };

            var spoolJobId = await _printingService.SendToPrinterAsync(request);
            
            printJob.StartPrinting(spoolJobId, pageCount * printJob.Copies);
            
            if (_activeJobs.TryGetValue(printJobId, out var jobInfo))
            {
                jobInfo.SpoolJobId = spoolJobId;
                jobInfo.Status = "Processing";
            }
            
            printer.SetStatus(PrinterStatus.Busy);
            printer.UpdateJobsInQueue((printer.CurrentJobsInQueue ?? 0) + 1);
            
            await unitOfWork.CompleteAsync();
            
            _logger.LogInformation(
                "Задание с Id: {JobId} отправлено в принтер: {Printer} (Id в очереди: {SpoolId})", 
                printJobId, printer.Name, spoolJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в процессе выполнения задания {JobId}", printJobId);
            
            var printJob = await unitOfWork.PrintJobs.GetByIdAsync(printJobId);
            printJob?.SetError(ex.Message);
            await unitOfWork.CompleteAsync();
            
            _activeJobs.Remove(printJobId);
        }
    }

    private async Task<Printer?> SelectPrinterAsync(PrintJob printJob, IUnitOfWork unitOfWork)
    {
        var printers = await unitOfWork.Printers.GetAvailablePrintersAsync();
        
        var suitablePrinters = printers
            .Where(p => p.CanPrint(printJob.Format))
            .Where(p => !printJob.IsColor || p.IsColorSupported)
            .OrderBy(p => p.Status == PrinterStatus.Online ? 0 : 1)
            .ThenBy(p => p.CurrentJobsInQueue ?? 0)
            .ThenBy(p => p.PagesPrinted)
            .ToList();

        return suitablePrinters.FirstOrDefault();
    }

    private async Task<string> PrepareFileAsync(string filePath, string fileType)
    {
        if (fileType == "pdf") return filePath;
        _logger.LogInformation("Конвертация файла {File} из {Type} в PDF", filePath, fileType);
        return await _printingService.ConvertToPdfAsync(filePath);

    }

    private async void CheckActiveJobs(object? state)
    {
        if (_activeJobs.Count == 0)
            return;

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var completedJobs = new List<Guid>();

        foreach (var kvp in _activeJobs.ToList())
        {
            try
            {
                if (string.IsNullOrEmpty(kvp.Value.SpoolJobId))
                    continue;

                var status = await _printingService.GetPrintJobStatusAsync(kvp.Value.SpoolJobId);
                
                if (status == null || status.Status == "completed")
                {
                    var printJob = await unitOfWork.PrintJobs.GetByIdAsync(kvp.Key);
                    if (printJob is { Status: PrintStatus.InProgress })
                    {
                        printJob.Complete();
                        
                        if (printJob.PrinterId.HasValue)
                        {
                            var printer = await unitOfWork.Printers.GetByIdAsync(printJob.PrinterId.Value);
                            if (printer != null)
                            {
                                printer.SetStatus(PrinterStatus.Online);
                                printer.UpdateJobsInQueue(Math.Max(0, (printer.CurrentJobsInQueue ?? 1) - 1));
                                printer.IncrementPageCount(printJob.TotalPages ?? 1);
                            }
                        }
                        
                        completedJobs.Add(kvp.Key);
                        _logger.LogInformation("Job {JobId} completed", kvp.Key);
                    }
                }
                else if (status.PrintedPages > 0)
                {
                    var printJob = await unitOfWork.PrintJobs.GetByIdAsync(kvp.Key);
                    printJob?.UpdateProgress(status.PrintedPages);
                }

                await unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки задания с Id: {JobId}", kvp.Key);
            }
        }

        foreach (var jobId in completedJobs)
        {
            _activeJobs.Remove(jobId);
        }
    }

    public override void Dispose()
    {
        _statusCheckTimer?.Dispose();
        base.Dispose();
    }

    private class JobInfo
    {
        public string? SpoolJobId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime EnqueuedAt { get; set; }
    }
}

public class QueueStatusInfo
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public List<JobStatusInfo> Jobs { get; set; } = [];
}

public class JobStatusInfo
{
    public Guid JobId { get; set; }
    public int Position { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnqueuedAt { get; set; }
}