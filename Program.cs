using Microsoft.EntityFrameworkCore;
using PrintingTools.Extensions;
using PrintingTools.Infrastructure.Data;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("AppEnvironments.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularAppCors", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrintingDbContext>();
    
    try
    {
        Log.Information("Применяются миграции в бд...");
        await context.Database.MigrateAsync();
        
        Log.Information("Загружаются изначальные данные...");
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        
        Log.Information("Инициализация базы данных успешно прошла.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Произошла ошибка во время инициализации базы данных.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Theme = ScalarTheme.DeepSpace;
        options.Layout = ScalarLayout.Modern;
        options.Title = "Printing Tools";
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AngularAppCors");
app.UseAuthentication();
app.UseAuthorization();

Log.Information("Запускается приложение PrintingTools API...");
app.Run();