using System.Diagnostics;
using System.Text.RegularExpressions;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Services.Models;

namespace PrintingTools.Infrastructure.Services;

public partial class CupsPrintingService : IPrintingService
{
    private readonly ILogger<CupsPrintingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _cupsServer;
    private readonly int _cupsPort;
    
    public CupsPrintingService(
        ILogger<CupsPrintingService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _cupsServer = configuration["PrintingSettings:CupsServer"] ?? "localhost";
        _cupsPort = configuration.GetValue<int>("PrintingSettings:CupsPort", 631);
    }
    
   public async Task<List<PrinterInfo>> GetAvailablePrintersAsync()
    {
        var printers = new List<PrinterInfo>();
        
        try
        {
            var process = CreateProcess("lpstat", $"-h {_cupsServer}:{_cupsPort} -p -d");
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("lpstat stderr: {Error}", error);
            }
            
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string? defaultPrinter = null;
            
            foreach (var line in lines)
            {
                if (line.StartsWith("system default destination:"))
                {
                    defaultPrinter = line.Replace("system default destination:", "").Trim();
                }
                else if (line.StartsWith("printer"))
                {
                    var match = MyRegex().Match(line);
                    if (!match.Success) continue;
                    var name = match.Groups[1].Value;
                    var status = match.Groups[2].Value;
                    var enabled = match.Groups[3].Value == "enabled";
                        
                    printers.Add(new PrinterInfo
                    {
                        Name = name,
                        Status = status,
                        IsOnline = enabled,
                        IsDefault = name == defaultPrinter,
                        JobsInQueue = await GetJobsCountAsync(name),
                        SupportedFormats = ["pdf", "ps", "txt", "jpg", "png"]
                    });
                }
            }
            
            _logger.LogInformation("Найдено {Count} количество принтеров в CUPS", printers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения принтеров из CUPS сервера");
        }
        
        return printers;
    }

    public async Task<string> SendToPrinterAsync(PrintJobRequest request)
    {
        try
        {
            var fileToPrint = request.FilePath;
            if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileToPrint = await ConvertToPdfAsync(request.FilePath);
            }
            
            var jobName = $"PrintJob_{Guid.NewGuid():N}";
            var args = $"-h {_cupsServer}:{_cupsPort} -d {request.PrinterName} -n {request.Copies}";
            
            if (request.IsDuplex)
                args += " -o sides=two-sided-long-edge";
            else
                args += " -o sides=one-sided";
            
            if (!request.IsColor)
                args += " -o ColorModel=Gray";
            
            args += request.Quality switch
            {
                PrintQuality.Draft => " -o print-quality=3",
                PrintQuality.Normal => " -o print-quality=4",
                PrintQuality.High => " -o print-quality=5",
                PrintQuality.Photo => " -o print-quality=5 -o media=photographic",
                _ => " -o print-quality=4"
            };
            
            if (!string.IsNullOrEmpty(request.PageRange))
                args += $" -o page-ranges={request.PageRange}";
            
            args += $" -t \"{jobName}\" \"{fileToPrint}\"";
            
            var process = CreateProcess("lp", args);
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                throw new Exception($"lp завершился с кодом ошибки {process.ExitCode}: {error}");
            }
            
            var match = Regex.Match(output, @"request id is (\S+)");
            var spoolJobId = match.Success ? match.Groups[1].Value : jobName;
            
            _logger.LogInformation("Задача: {JobId} отправлено на принтер: {Printer}", spoolJobId, request.PrinterName);
            
            return spoolJobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправки задачи на печать в принтер");
            throw;
        }
    }

    public async Task<PrintJobStatus?> GetPrintJobStatusAsync(string spoolJobId)
    {
        try
        {
            var process = CreateProcess("lpstat", $"-h {_cupsServer}:{_cupsPort} -o {spoolJobId}");
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (string.IsNullOrWhiteSpace(output))
                return null; 
            
            var status = new PrintJobStatus
            {
                JobId = spoolJobId,
                Status = "unknown"
            };
            
            if (output.Contains("pending"))
                status.Status = "pending";
            else if (output.Contains("processing"))
                status.Status = "processing";
            else if (output.Contains("stopped"))
                status.Status = "stopped";
            else if (output.Contains("canceled"))
                status.Status = "canceled";
            else if (output.Contains("completed"))
                status.Status = "completed";
            
            var sizeMatch = Regex.Match(output, @"(\d+)k");
            if (!sizeMatch.Success) return status;
            var sizeKb = int.Parse(sizeMatch.Groups[1].Value);
            status.TotalPages = Math.Max(1, sizeKb / 50); // Примерная оценка

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения статуса задачи для задачи с Id: {JobId}", spoolJobId);
            return null;
        }
    }

    public async Task<bool> CancelPrintJobAsync(string spoolJobId)
    {
        try
        {
            var process = CreateProcess("cancel", $"-h {_cupsServer}:{_cupsPort} {spoolJobId}");
            
            process.Start();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                _logger.LogWarning("Ошибка при попытке завершения задачи с Id {JobId}: {Error}", spoolJobId, error);
                return false;
            }
            
            _logger.LogInformation("Задача {JobId} успешно завершена", spoolJobId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при попытке завершения задачи с Id {JobId}", spoolJobId);
            return false;
        }
    }

    public async Task<bool> PausePrintJobAsync(string spoolJobId)
    {
        try
        {
            var process = CreateProcess("lp", $"-h {_cupsServer}:{_cupsPort} -i {spoolJobId} -H hold");
            
            process.Start();
            await process.WaitForExitAsync();
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при приостановке задачи с Id {JobId}", spoolJobId);
            return false;
        }
    }

    public async Task<bool> ResumePrintJobAsync(string spoolJobId)
    {
        try
        {
            var process = CreateProcess("lp", $"-h {_cupsServer}:{_cupsPort} -i {spoolJobId} -H resume");
            
            process.Start();
            await process.WaitForExitAsync();
            
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при возобновления задачи с Id {JobId}", spoolJobId);
            return false;
        }
    }

    public async Task<int> GetPageCountAsync(string filePath)
    {
        try
        {
            if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return 1;
            var process = CreateProcess("pdfinfo", $"\"{filePath}\"");
                
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
                
            var match = Regex.Match(output, @"Pages:\s+(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting pages in {File}", filePath);
            return 1;
        }
    }

    public async Task<string> ConvertToPdfAsync(string filePath)
    {
        try
        {
            var outputPath = Path.ChangeExtension(filePath, ".pdf");
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            Process process;
            
            switch (extension)
            {
                case ".docx":
                case ".doc":
                case ".odt":
                case ".xls":
                case ".xlsx":
                case ".ods":
                {
                    process = CreateProcess("libreoffice", 
                        $"--headless --convert-to pdf --outdir \"{Path.GetDirectoryName(filePath)}\" \"{filePath}\"");
                
                    process.Start();
                    await process.WaitForExitAsync();
                
                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        throw new Exception($"LibreOffice conversion failed: {error}");
                    }

                    break;
                }
                case ".txt":
                {
                    var psPath = Path.ChangeExtension(filePath, ".ps");
                    process = CreateProcess("a2ps", 
                        $"-B --no-header -o \"{psPath}\" \"{filePath}\"");
                
                    process.Start();
                    await process.WaitForExitAsync();
                
                    process = CreateProcess("ps2pdf", $"\"{psPath}\" \"{outputPath}\"");
                    process.Start();
                    await process.WaitForExitAsync();
                
                    if (File.Exists(psPath))
                        File.Delete(psPath);
                    break;
                }
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                    process = CreateProcess("convert", $"\"{filePath}\" \"{outputPath}\"");
                    process.Start();
                    await process.WaitForExitAsync();
                    break;
                default:
                    return filePath;
            }
            
            _logger.LogInformation("Converted {Input} to PDF: {Output}", filePath, outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting {File} to PDF", filePath);
            throw;
        }
    }

    public async Task<PrinterInfo?> GetPrinterStatusAsync(string printerName)
    {
        try
        {
            var process = CreateProcess("lpstat", $"-h {_cupsServer}:{_cupsPort} -p {printerName}");
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (string.IsNullOrWhiteSpace(output))
                return null;
            
            var info = new PrinterInfo
            {
                Name = printerName,
                JobsInQueue = await GetJobsCountAsync(printerName)
            };
            
            if (output.Contains("idle"))
                info.Status = "idle";
            else if (output.Contains("printing"))
                info.Status = "printing";
            else if (output.Contains("stopped"))
                info.Status = "stopped";
            
            info.IsOnline = output.Contains("enabled");
            
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения статуса принтера: {Printer}", printerName);
            return null;
        }
    }

    private async Task<int> GetJobsCountAsync(string printerName)
    {
        try
        {
            var process = CreateProcess("lpstat", $"-h {_cupsServer}:{_cupsPort} -o {printerName}");
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            return lines.Length;
        }
        catch
        {
            return 0;
        }
    }

    private Process CreateProcess(string command, string arguments)
    {
        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    }

    [GeneratedRegex(@"printer\s+(\S+)\s+.*is\s+(\w+)\.\s+(\w+)")]
    private static partial Regex MyRegex();
    
}