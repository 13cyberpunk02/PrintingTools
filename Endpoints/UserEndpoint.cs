using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Application.Services;
using PrintingTools.Common.Filters;

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
            .Produces<ApiResponse<UserDto>>(404);;
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
}