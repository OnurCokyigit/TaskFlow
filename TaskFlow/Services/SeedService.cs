using TaskFlow.Data;
using TaskFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Services
{
    public class SeedService
    {
        private readonly PasswordService _passwordService;

        public SeedService()
        {
            _passwordService = new PasswordService();
        }

        public async Task SeedAsync()
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();

            // Kategorileri ayrı kontrol et — admin kontrolünden bağımsız
            if (!await db.Categories.AnyAsync())
            {
                var categories = new List<Category>
        {
            new Category { Name = "Geliştirme",     ColorHex = "#3498DB" },
            new Category { Name = "Tasarım",        ColorHex = "#9B59B6" },
            new Category { Name = "Test",           ColorHex = "#E67E22" },
            new Category { Name = "Dokümantasyon",  ColorHex = "#27AE60" },
            new Category { Name = "Toplantı",       ColorHex = "#E74C3C" },
            new Category { Name = "Araştırma",      ColorHex = "#1ABC9C" }
        };
                db.Categories.AddRange(categories);
                await db.SaveChangesAsync();
            }

            // Admin kontrolü
            if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
                return;

            // Varsayılan departman
            var department = new Department { Name = "Genel" };
            db.Departments.Add(department);
            await db.SaveChangesAsync();

            // Varsayılan takım
            var team = new Team
            {
                Name = "Yönetim",
                DepartmentId = department.Id
            };
            db.Teams.Add(team);
            await db.SaveChangesAsync();

            // Varsayılan Admin
            var admin = new User
            {
                Username = "admin",
                PasswordHash = new PasswordService().HashPassword("admin123"),
                FullName = "Sistem Yöneticisi",
                Role = UserRole.Admin,
                DepartmentId = department.Id,
                TeamId = team.Id
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }
}