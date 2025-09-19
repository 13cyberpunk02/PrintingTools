using Microsoft.AspNetCore.Mvc;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.PrintJobs;
using PrintingTools.Application.Services;
using PrintingTools.Extensions;

namespace PrintingTools.Endpoints;

public static class PrintJobEndpoint
{
    public static IEndpointRouteBuilder MapPrintJobEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("api/print-jobs")
            .WithTags("Print Jobs")
            .RequireAuthorization()
            .WithOpenApi();
        
        group.MapGet("/", GetPrintJobs)
            .WithName("GetPrintJobs")
            .Produces<ApiResponse<PrintJobsListDto>>();

        group.MapPost("/", CreatePrintJob)
            .WithName("CreatePrintJob")
            .Produces<ApiResponse<PrintJobDto>>(201)
            .Produces<ApiResponse<PrintJobDto>>(400)
            .WithRequestValidation<CreatePrintJobDto >()
            .DisableAntiforgery();
        
        group.MapGet("/{id:guid}", GetPrintJobById)
            .WithName("GetPrintJobById")
            .Produces<ApiResponse<PrintJobDto>>()
            .Produces<ApiResponse<PrintJobDto>>(404);
        
        group.MapPut("/{id:guid}/cancel", CancelPrintJob)
            .WithName("CancelPrintJob")
            .Produces<ApiResponse<PrintJobDto>>()
            .Produces<ApiResponse<PrintJobDto>>(400);

        group.MapPut("/{id:guid}/start", StartPrinting)
            .RequireAuthorization("AdminOnly")
            .WithName("StartPrinting")
            .Produces<ApiResponse<PrintJobDto>>();
        
        group.MapPut("/{id:guid}/complete", CompletePrintJob)
            .RequireAuthorization("AdminOnly")
            .WithName("CompletePrintJob")
            .Produces<ApiResponse<PrintJobDto>>();
        
        group.MapGet("/statistics", GetPrintStatistics)
            .WithName("GetPrintStatistics")
            .Produces<ApiResponse<PrintJobStatisticsDto>>();
        
        return group;
    }

    private static async Task<IResult> GetPrintJobs(
        HttpContext context,
        [AsParameters] PagedRequest request,
        IPrintService printService)
    {
        var isAdmin = context.User.IsAdmin();
        var userId = isAdmin ? (Guid?)null : context.User.GetUserId();
    
        var result = await printService.GetPrintJobsAsync(request, userId);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPrintJobById(
        Guid id,
        HttpContext context,
        IPrintService printService)
    {
        var userId = context.User.GetUserId();
        var isAdmin = context.User.IsAdmin();
    
        var result = await printService.GetPrintJobByIdAsync(id, userId, isAdmin);
        return result.Success 
            ? Results.Ok(result) 
            : Results.NotFound(result);
    }

    private static async Task<IResult> CreatePrintJob(
        HttpContext context,
        [FromForm] CreatePrintJobDto request,
        IPrintService printService)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await printService.CreatePrintJobAsync(userId, request);
        return result.Success 
            ? Results.Created($"/api/print-jobs/{result.Data?.Id}", result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> CancelPrintJob(
        Guid id,
        HttpContext context,
        IPrintService printService)
    {
        var userId = context.User.GetUserId();
        var isAdmin = context.User.IsAdmin();
    
        var result = await printService.CancelPrintJobAsync(id, userId, isAdmin);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> StartPrinting(
        Guid id,
        [FromBody] StartPrintingRequest request,
        IPrintService printService)
    {
        var result = await printService.StartPrintingAsync(id, request.PrinterName);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> CompletePrintJob(
        Guid id,
        [FromBody] CompletePrintJobRequest request,
        IPrintService printService)
    {
        var result = await printService.CompletePrintJobAsync(id, request.Cost);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPrintStatistics(
        HttpContext context,
        [AsParameters]StatisticsRequest request,
        IPrintService printService)
    {
        var isAdmin = context.User.IsAdmin();
        var userId = isAdmin && request.UserId.HasValue ? request.UserId : context.User.GetUserId();
    
        var result = await printService.GetStatisticsAsync(
            isAdmin ? null : userId, 
            request.From, 
            request.To);
    
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }
}