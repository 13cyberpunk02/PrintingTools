using Microsoft.AspNetCore.Mvc;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Application.Services;
using PrintingTools.Extensions;

namespace PrintingTools.Endpoints;

public static class ProfileEndpoint
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("api/profile")
            .WithTags("Profile")
            .RequireAuthorization()
            .WithOpenApi();
        
        group.MapGet("/", GetMyProfile)
            .WithName("GetMyProfile")
            .Produces<ApiResponse<UserProfileDto>>()
            .Produces(401);

        group.MapPut("/", UpdateMyProfile)
            .WithName("UpdateMyProfile")
            .Produces<ApiResponse<UserDto>>(200)
            .Produces(401)
            .WithRequestValidation<UpdateUserDto>();
        
        return group;
    }

    private static async Task<IResult> GetMyProfile(
        HttpContext context,
        IUserService userService)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await userService.GetProfileAsync(userId);
        return result.Success 
            ? Results.Ok(result) 
            : Results.NotFound(result);
    }

    private static async Task<IResult> UpdateMyProfile( 
        HttpContext context,
        [FromBody] UpdateUserDto request,        
        IUserService userService)
    {
        var userId = context.User.GetUserId();
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var result = await userService.UpdateUserAsync(userId, request);
        return result.Success 
            ? Results.Ok(result) 
            : Results.BadRequest(result);
    }
}