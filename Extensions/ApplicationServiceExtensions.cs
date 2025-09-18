using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.Mappings;
using PrintingTools.Application.Services;

namespace PrintingTools.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                var response = new ErrorResponse
                {
                    Message = "Ошибка валидации",
                    Errors = errors,
                    TraceId = context.HttpContext.TraceIdentifier
                };
                return new BadRequestObjectResult(response);
            };
        });
        return services;
    }
}