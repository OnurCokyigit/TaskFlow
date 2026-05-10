using TaskFlow.Data;
using TaskFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Services
{
    public class AuthService
    {
        private readonly PasswordService _passwordService;

        // Giriş yapan kullanıcıyı tüm uygulama boyunca tutar
        public User? CurrentUser { get; private set; }

        public AuthService()
        {
            _passwordService = new PasswordService();
        }

        // Kullanıcı girişi
        public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Kullanıcı adı ve şifre boş olamaz.");

            using var db = new AppDbContext();

            var user = await db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return (false, "Kullanıcı bulunamadı.");

            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
                return (false, "Şifre hatalı.");

            CurrentUser = user;
            SessionService.SetUser(user);
            return (true, "Giriş başarılı.");
        }

        // Çıkış
        public void Logout()
        {
            CurrentUser = null;
        }

        // Yetki kontrolü
        public bool IsAdmin() => CurrentUser?.Role == UserRole.Admin;
        public bool IsDepartmentManager() => CurrentUser?.Role == UserRole.DepartmentManager;
        public bool IsTeamLead() => CurrentUser?.Role == UserRole.TeamLead;
        public bool IsEmployee() => CurrentUser?.Role == UserRole.Employee;

        // Admin veya Departman Yöneticisi mi?
        public bool HasManagerAccess() =>
            IsAdmin() || IsDepartmentManager();

        // En az Takım Kaptanı yetkisi var mı?
        public bool HasTeamLeadAccess() =>
            IsAdmin() || IsDepartmentManager() || IsTeamLead();
    }
}