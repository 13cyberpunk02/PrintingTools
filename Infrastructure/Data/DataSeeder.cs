using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.Services;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Data;

public class DataSeeder
{
    private readonly PrintingDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    
    public DataSeeder(PrintingDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    
    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return; // База уже содержит данные

        // Создаем администратора по умолчанию
        var adminUser = new User(
            email: "admin@domain.ru",
            passwordHash: _passwordHasher.HashPassword("Qwerty12345!"), 
            firstName: "System",
            lastName: "Administrator",
            role: UserRole.Administrator
        );
        
        adminUser.ConfirmEmail(adminUser.EmailConfirmationToken!);

        _context.Users.Add(adminUser);
        
        var testUser = new User(
            email: "user@domain.ru",
            passwordHash: _passwordHasher.HashPassword("User12345!"),
            firstName: "User",
            lastName: "User",
            role: UserRole.User
        );
        
        testUser.ConfirmEmail(testUser.EmailConfirmationToken!);

        _context.Users.Add(testUser);

        await _context.SaveChangesAsync();
    }
}