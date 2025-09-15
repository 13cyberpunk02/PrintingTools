using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrintingTools.Domain.Services;
using PrintingTools.Infrastructure.Data;
using PrintingTools.Infrastructure.Repositories;
using PrintingTools.Infrastructure.Services;
using PrintingTools.Settings;

namespace PrintingTools.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDatabase(configuration);
        
        // Repositories
        services.AddRepositories();
        
        // Services
        services.AddDomainServices();
        
        // JWT Authentication
        services.AddJwtAuthentication(configuration);
        
        return services;
    }
    
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()
                               ?? throw new InvalidOperationException("DatabaseSettings not configured");

        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        services.AddDbContext<PrintingDbContext>(options =>
        {
            options.UseNpgsql(databaseSettings.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(databaseSettings.CommandTimeout);
                npgsqlOptions.EnableRetryOnFailure(3);
            });

            if (databaseSettings.EnableDetailedErrors)
                options.EnableDetailedErrors();
            
            if (databaseSettings.EnableSensitiveDataLogging)
                options.EnableSensitiveDataLogging();
        });

        // Data Seeder
        services.AddScoped<DataSeeder>();

        return services;
    }
    
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPrintJobRepository, PrintJobRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                          ?? throw new InvalidOperationException("JWT не настроено в переменных окружения.");
        
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
                    
            };
        });
        
        return services;
    }
}