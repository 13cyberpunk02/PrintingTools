using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.Services;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Data;

public class DataSeeder
{
    private readonly PrintingDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        PrintingDbContext context, 
        IPasswordHasher passwordHasher,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedUsersAsync();
            await SeedPrintersAsync();
            _logger.LogInformation("Внесение первоначальных пользователей в БД прошла успешно.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка, в процессе внесения первоначальных изменений в БД.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("В БД есть пользователи, пропускаем добавление первоначальных данных.");
            return;
        }
        
        var adminUser = new User(
            email: "admin@domain.ru",
            passwordHash: _passwordHasher.HashPassword("Admin123!"),
            firstName: "System",
            lastName: "Administrator",
            role: UserRole.Administrator
        );
        
        adminUser.ConfirmEmail(adminUser.EmailConfirmationToken!);

        _context.Users.Add(adminUser);
        
        var testUser = new User(
            email: "user@pdomain.ru",
            passwordHash: _passwordHasher.HashPassword("User123!"),
            firstName: "Test",
            lastName: "User",
            role: UserRole.User,
            phoneNumber: "+7 (999) 123-45-67"
        );
        
        testUser.ConfirmEmail(testUser.EmailConfirmationToken!);

        _context.Users.Add(testUser);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Первоначальные пользователи добавлены");
    }

    private async Task SeedPrintersAsync()
    {
        if (await _context.Printers.AnyAsync())
        {
            _logger.LogInformation("В БД уже есть принтеры, внесение первоначальных принтеров пропускаем");
            return;
        }

        var printers = new List<Printer>
        {
            new Printer(
                name: "HP_LaserJet_M428",
                model: "HP LaserJet Pro M428fdw",
                location: "Офис 201",
                type: PrinterType.Laser,
                networkPath: "192.168.1.100",
                isColorSupported: false,
                maxPaperWidth: 210,  // A4
                maxPaperHeight: 297
            ),
            
            new Printer(
                name: "Canon_Plotter",
                model: "Canon imagePROGRAF TM-300",
                location: "Офис 205",
                type: PrinterType.Plotter,
                networkPath: "192.168.1.101",
                isColorSupported: true,
                maxPaperWidth: 914,  // 36 inches
                maxPaperHeight: 50000  // Roll paper
            ),
            
            new Printer(
                name: "Epson_Color",
                model: "Epson WorkForce Pro WF-C5790",
                location: "Офис 203",
                type: PrinterType.Inkjet,
                networkPath: "192.168.1.102",
                isColorSupported: true,
                maxPaperWidth: 297,  // A3
                maxPaperHeight: 420
            )
        };
        
        printers[0].SetAsDefault();

        _context.Printers.AddRange(printers);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Первоначальные принтеры добавлены.");
    }
}