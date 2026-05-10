using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Helpers;
using TaskFlow.Models;
using TaskFlow.Services;

namespace TaskFlow.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly User _currentUser;

        public ProfileViewModel()
        {
            _db = new AppDbContext();
            _currentUser = SessionService.CurrentUser!;
            LoadDataAsync().ConfigureAwait(false);
        }

        // Kullanıcı bilgileri
        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string roleDisplay = string.Empty;
        [ObservableProperty] private string departmentName = string.Empty;
        [ObservableProperty] private string teamName = string.Empty;
        [ObservableProperty] private string avatarLetter = string.Empty;

        // Projeler
        [ObservableProperty] private List<Project> myProjects = new();

        private async Task LoadDataAsync()
        {
            // Kullanıcıyı ilişkileriyle getir
            var user = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.Id == _currentUser.Id);

            if (user == null) return;

            FullName = user.FullName;
            Username = user.Username;
            AvatarLetter = user.FullName.Length > 0
                ? user.FullName[0].ToString().ToUpper()
                : "?";
            RoleDisplay = GetRoleDisplay(user.Role);
            DepartmentName = user.Department?.Name ?? "—";
            TeamName = user.Team?.Name ?? "—";

            // Kullanıcının atandığı projeler
            MyProjects = await _db.ProjectMembers
                .Where(pm => pm.UserId == user.Id)
                .Include(pm => pm.Project)
                    .ThenInclude(p => p.Team)
                .Select(pm => pm.Project)
                .ToListAsync();
        }

        [RelayCommand]
        private void OpenProject(Project project)
        {
            // İleride proje detay ekranına geçiş yapacak
            // Şimdilik boş bırakıyoruz
        }

        [RelayCommand]
        private void Logout()
        {
            SessionService.Clear();
            NavigationService.NavigateTo<MainWindow>();
        }

        private string GetRoleDisplay(UserRole role) => role switch
        {
            UserRole.Admin => "Yönetici",
            UserRole.DepartmentManager => "Departman Yöneticisi",
            UserRole.TeamLead => "Takım Kaptanı",
            UserRole.Employee => "Çalışan",
            _ => string.Empty
        };
    }
}