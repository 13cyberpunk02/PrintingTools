using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PrintingTools.Application.DTOs.Auth;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.Services;
using PrintingTools.Application.Validators.Auth;
using PrintingTools.Common.Filters;
using PrintingTools.Extensions;

namespace PrintingTools.Endpoints;

public static class AuthEndpoint
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/auth")
            .WithTags("Authentication")
            .AddEndpointFilter<RequestLoggingFilter>()
            .WithOpenApi();
        
        group.MapPost("login", LoginAsync)
            .WithName("Login")
            .WithSummary("Авторизация пользователя по email и паролю")
            .WithRequestValidation<LoginRequest>()
            .Produces<ApiResponse<LoginResponse>>(400)
            .Produces<ApiResponse<LoginResponse>>();
        
        group.MapPost("register", RegisterAsync)
            .WithName("Register")
            .WithRequestValidation<RegisterRequest>();
        
        group.MapPost("refresh-token", RefreshTokenAsync)
            .WithName("Refresh-Token");
        
        return group;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody]LoginRequest request,     
        IAuthService authService,
        CancellationToken cancellationToken = default)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response.Success
            ? Results.Ok(response)
            : Results.BadRequest(response);
    }

    private static async Task<IResult> RegisterAsync(IAuthService authService, RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return response.Success
            ? Results.Ok(response)
            : Results.BadRequest(response);
    }

    private static async Task<IResult> RefreshTokenAsync(IAuthService authService, RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await authService.RefreshTokenAsync(request, cancellationToken);
        return response.Success 
            ? Results.Ok(response)
            : Results.BadRequest(response);
    }
}