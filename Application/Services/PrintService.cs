using AutoMapper;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.PrintJobs;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Repositories;

namespace PrintingTools.Application.Services;

public class PrintService : IPrintService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PrintService> _logger;

    public PrintService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IWebHostEnvironment environment,
        ILogger<PrintService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _environment = environment;
        _logger = logger;
    }
    
    public async Task<ApiResponse<PrintJobDto>> CreatePrintJobAsync(Guid userId, CreatePrintJobDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Пользователь не найден");
            }

            if (!user.CanPrint())
            {
                return ApiResponse<PrintJobDto>.Fail("У вас нет прав на создание заданий печати");
            }

            if (!Enum.TryParse<PrintFormat>(dto.Format, true, out var format))
            {
                return ApiResponse<PrintJobDto>.Fail("Некорректный формат печати");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents", userId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream, cancellationToken);
            }

            var printJob = new PrintJob(
                userId: userId,
                fileName: dto.File.FileName,
                filePath: filePath,
                fileSizeBytes: dto.File.Length,
                format: format,
                copies: dto.Copies
            );

            await _unitOfWork.PrintJobs.AddAsync(printJob, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            
            var jobWithUser = await _unitOfWork.PrintJobs.GetByIdAsync(printJob.Id);
            
            _logger.LogInformation("Создано задание печати {JobId} для пользователя {UserId}", printJob.Id, userId);
            
            var jobDto = _mapper.Map<PrintJobDto>(jobWithUser);
            return ApiResponse<PrintJobDto>.Ok(jobDto, "Задание печати создано");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании задания печати для пользователя {UserId}", userId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка при создании задания печати");
        }
    }

    public async Task<ApiResponse<PrintJobDto>> GetPrintJobByIdAsync(Guid jobId, Guid userId, bool isAdmin)
    {
        try
        {
            var job = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (job == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Задание печати не найдено");
            }
            
            if (!isAdmin && job.UserId != userId)
            {
                return ApiResponse<PrintJobDto>.Fail("У вас нет доступа к этому заданию");
            }

            var jobDto = _mapper.Map<PrintJobDto>(job);
            return ApiResponse<PrintJobDto>.Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении задания печати {JobId}", jobId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка при получении задания печати");
        }
    }

    public async Task<ApiResponse<PrintJobsListDto>> GetPrintJobsAsync(PagedRequest request, Guid? userId = null)
    {
        try
        {
            var jobs = userId.HasValue
                ? await _unitOfWork.PrintJobs.GetByUserIdAsync(userId.Value)
                : await _unitOfWork.PrintJobs.GetAllAsync();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                jobs = jobs.Where(j => j.FileName.ToLower().Contains(searchTerm));
            }

            // Сортировка
            jobs = request.SortBy?.ToLower() switch
            {
                "filename" => request.SortDescending ? 
                    jobs.OrderByDescending(j => j.FileName) : 
                    jobs.OrderBy(j => j.FileName),
                "status" => request.SortDescending ? 
                    jobs.OrderByDescending(j => j.Status) : 
                    jobs.OrderBy(j => j.Status),
                "createdat" => request.SortDescending ? 
                    jobs.OrderByDescending(j => j.CreatedAt) : 
                    jobs.OrderBy(j => j.CreatedAt),
                _ => jobs.OrderByDescending(j => j.CreatedAt)
            };

            var printJobs = jobs as PrintJob[] ?? jobs.ToArray();
            var totalCount = printJobs.Length;
            var skip = (request.PageNumber - 1) * request.PageSize;
            var pagedJobs = printJobs.Skip(skip).Take(request.PageSize).ToList();

            var jobIds = pagedJobs.Select(j => j.Id).ToList();
            var jobsWithUsers = new List<PrintJob>();
            foreach (var id in jobIds)
            {
                var job = await _unitOfWork.PrintJobs.GetByIdAsync(id);
                if (job != null) jobsWithUsers.Add(job);
            }

            var response = new PrintJobsListDto
            {
                PrintJobs = _mapper.Map<List<PrintJobDto>>(jobsWithUsers),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<PrintJobsListDto>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка заданий печати");
            return ApiResponse<PrintJobsListDto>.Fail("Произошла ошибка при получении списка заданий");
        }
    }

    public async Task<ApiResponse<PrintJobsListDto>> GetUserPrintJobsAsync(Guid userId, PagedRequest request)
     => await GetPrintJobsAsync(request, userId);

    public async Task<ApiResponse<PrintJobDto>> StartPrintingAsync(Guid jobId, string printerName, CancellationToken  cancellationToken = default)
    {
        try
        {
            var job = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (job == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Задание печати не найдено");
            }

            job.StartPrinting(printerName);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Начата печать задания {JobId} на принтере {Printer}", jobId, printerName);
            
            var jobDto = _mapper.Map<PrintJobDto>(job);
            return ApiResponse<PrintJobDto>.Ok(jobDto, "Печать начата");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<PrintJobDto>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при начале печати задания {JobId}", jobId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка при начале печати");
        }
    }

    public async Task<ApiResponse<PrintJobDto>> CompletePrintJobAsync(Guid jobId, decimal cost, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (job == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Задание печати не найдено");
            }

            job.Complete(cost);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Завершено задание печати {JobId} со стоимостью {Cost}", jobId, cost);
            
            var jobDto = _mapper.Map<PrintJobDto>(job);
            return ApiResponse<PrintJobDto>.Ok(jobDto, "Печать завершена");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<PrintJobDto>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при завершении задания печати {JobId}", jobId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка при завершении печати");
        }
    }

    public async Task<ApiResponse<PrintJobDto>> CancelPrintJobAsync(Guid jobId, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (job == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Задание печати не найдено");
            }

            if (!isAdmin && job.UserId != userId)
            {
                return ApiResponse<PrintJobDto>.Fail("У вас нет прав на отмену этого задания");
            }

            job.Cancel();
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Отменено задание печати {JobId} пользователем {UserId}", jobId, userId);
            
            var jobDto = _mapper.Map<PrintJobDto>(job);
            return ApiResponse<PrintJobDto>.Ok(jobDto, "Задание отменено");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<PrintJobDto>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отмене задания печати {JobId}", jobId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка при отмене задания");
        }
    }

    public async Task<ApiResponse<PrintJobDto>> SetPrintJobErrorAsync(Guid jobId, string errorMessage, CancellationToken  cancellationToken = default)
    {
        try
        {
            var job = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (job == null)
            {
                return ApiResponse<PrintJobDto>.Fail("Задание печати не найдено");
            }

            job.SetError(errorMessage);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogError("Ошибка в задании печати {JobId}: {Error}", jobId, errorMessage);
            
            var jobDto = _mapper.Map<PrintJobDto>(job);
            return ApiResponse<PrintJobDto>.Ok(jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при установке ошибки для задания печати {JobId}", jobId);
            return ApiResponse<PrintJobDto>.Fail("Произошла ошибка");
        }
    }

    public async Task<ApiResponse<PrintJobStatisticsDto>> GetStatisticsAsync(Guid? userId = null, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var jobs = userId.HasValue
                ? await _unitOfWork.PrintJobs.GetByUserIdAsync(userId.Value)
                : await _unitOfWork.PrintJobs.GetAllAsync();
            
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow;
            jobs = jobs.Where(j => j.CreatedAt >= fromDate && j.CreatedAt <= toDate);

            var jobsList = jobs.ToList();

            var statistics = new PrintJobStatisticsDto
            (
                TotalJobs: jobsList.Count,
                PendingJobs: jobsList.Count(j => j.Status == PrintStatus.Pending),
                InProgressJobs: jobsList.Count(j => j.Status == PrintStatus.InProgress),
                CompletedJobs: jobsList.Count(j => j.Status == PrintStatus.Completed),
                FailedJobs: jobsList.Count(j => j.Status == PrintStatus.Failed),
                CancelledJobs: jobsList.Count(j => j.Status == PrintStatus.Cancelled),
                TotalCost: jobsList.Where(j => j.Cost.HasValue).Sum(j => j.Cost!.Value),
                TotalPrintedPages: jobsList.Where(j => j.Status == PrintStatus.Completed).Sum(j => j.Copies),
                JobsByFormat: jobsList.GroupBy(j => j.Format.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                DailyStats:  jobsList.GroupBy(j => j.CreatedAt.Date)
                    .Select(g => new DailyStatistics
                    (
                        Date: g.Key,
                        JobCount: g.Count(),
                        TotalCost: g.Where(j => j.Cost.HasValue).Sum(j => j.Cost!.Value)
                    ))
                    .OrderBy(d => d.Date)
                    .ToList()
            );

            return ApiResponse<PrintJobStatisticsDto>.Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики");
            return ApiResponse<PrintJobStatisticsDto>.Fail("Произошла ошибка при получении статистики");
        }
    }
}