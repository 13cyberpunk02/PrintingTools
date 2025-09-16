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
            .Produces<ApiResponse<LoginResponse>>()
            .Produces<ApiResponse<LoginResponse>>(400)
            .WithRequestValidation<RegisterRequest>();
        
        group.MapPost("refresh-token", RefreshTokenAsync)
            .WithName("Refresh-Token")
            .Produces<ApiResponse<TokenResponse>>()
            .Produces<ApiResponse<TokenResponse>>(400)
            .WithRequestValidation<RefreshTokenRequest>();
        
        group.MapPost("logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);
        
        group.MapPost("change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .Produces<ApiResponse<object>>(200)
            .Produces(401)
            .WithRequestValidation<ChangePasswordRequest>();
        
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

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        [FromBody] string refreshToken,
        IAuthService authService)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
            return Results.Unauthorized();
        var result = await authService.LogoutAsync(userId, refreshToken);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        HttpContext context,
        IAuthService authService,
        CancellationToken cancellationToken = default)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await authService.ChangePasswordAsync(userId, request, cancellationToken);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }
}