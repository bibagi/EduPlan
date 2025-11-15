using Schedule.Core.Data;
using Schedule.Core.Models;

namespace Schedule.Core.Services;

public class DatabaseInitializer
{
    private readonly ScheduleDbContext _context;

    public DatabaseInitializer(ScheduleDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        // Создаём БД если не существует
        await _context.Database.EnsureCreatedAsync();

        // Проверяем, есть ли пользователи
        if (!_context.Users.Any())
        {
            // Создаём единственного администратора
            var admin = new User
            {
                Login = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = "Admin"
            };
            
            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
            
            Console.WriteLine("✓ Создан администратор: admin / admin");
        }
        else
        {
            Console.WriteLine($"✓ База данных содержит {_context.Users.Count()} пользователей");
        }
    }
}
