using Microsoft.AspNetCore.Mvc;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Application.Services;
using PrintingTools.Common.Filters;
using PrintingTools.Extensions;

namespace PrintingTools.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/users")
            .WithTags("User")
            .RequireAuthorization("AdminOnly")
            .AddEndpointFilter<RequestLoggingFilter>()
            .WithOpenApi();

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .Produces<ApiResponse<UsersListDto>>();

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .Produces<ApiResponse<UserDto>>()
            .Produces<ApiResponse<UserDto>>(404);
        
        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .Produces<ApiResponse<UserDto>>(201)
            .Produces<ApiResponse<UserDto>>(400)
            .WithRequestValidation<CreateUserDto>();
        
        group.MapPut("/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .Produces<ApiResponse<UserDto>>()
            .Produces<ApiResponse<UserDto>>(400)
            .WithRequestValidation<UpdateUserDto>();
        
        group.MapPut("/{id:guid}/role", UpdateUserRoleAsync)
            .WithName("UpdateUserRole")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(400);

        group.MapPut("/{id:guid}/block", BlockUser)
            .WithName("BlockUser")
            .Produces<ApiResponse<object>>();
        
        group.MapDelete("/{id:guid}",DeleteUser)
            .WithName("DeleteUser")
            .Produces(204)
            .Produces<ApiResponse<object>>(400);
        
        return group;
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] PagedRequest request,
        IUserService userService)
    {
        var result = await userService.GetUsersAsync(request);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        IUserService userService)
    {
        var result = await userService.GetUserByIdAsync(id);
        return result.Success 
            ? Results.Ok(result) 
            : Results.NotFound(result);
    }

    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserDto request, 
        IUserService userService)
    {
        var result = await userService.CreateUserAsync(request);
        return result.Success 
            ? Results.Created($"/api/users/{result.Data?.Id}", result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserDto request,
        IUserService userService)
    {
        var result = await userService.UpdateUserAsync(id, request);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);        
    }

    private static async Task<IResult> UpdateUserRoleAsync(
        Guid id,
        [FromBody] UpdateUserRoleDto request,
        IUserService userService)
    {
        var result = await userService.UpdateUserRoleAsync(id, request);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);   
    }

    private static async Task<IResult> BlockUser(
        Guid id, 
        IUserService userService)
    {
        var result = await userService.BlockUserAsync(id);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        IUserService userService)
    {
        var result = await userService.DeleteUserAsync(id);
        return result.Success 
            ? Results.NoContent() 
            : Results.BadRequest(result);
    }
}