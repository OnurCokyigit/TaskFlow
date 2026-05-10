using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Helpers;
using TaskFlow.Models;
using TaskFlow.Services;
using TaskStatus = TaskFlow.Models.TaskStatus;

namespace TaskFlow.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly User _currentUser;

        public DashboardViewModel()
        {
            _db = new AppDbContext();
            _currentUser = SessionService.CurrentUser!;

            FullName = _currentUser.FullName;
            RoleDisplay = GetRoleDisplay(_currentUser.Role);

            LoadDataAsync().ConfigureAwait(false);
        }

        // Kullanıcı bilgileri
        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string roleDisplay = string.Empty;

        // İstatistikler
        [ObservableProperty] private int totalProjects;
        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int completedTasks;
        [ObservableProperty] private int totalUsers;
        [ObservableProperty] private int inProgressTasks;
        [ObservableProperty] private int todoTasks;
        [ObservableProperty] private double completionRate;
        [ObservableProperty] private string completionRateDisplay = "0%";

        // Listeler
        [ObservableProperty] private List<ProjectSummary> projectSummaries = new();
        [ObservableProperty] private List<User> recentUsers = new();
        [ObservableProperty] private List<Commit> recentCommits = new();

        private async Task LoadDataAsync()
        {
            var role = _currentUser.Role;

            // Role göre proje sorgusu
            IQueryable<Project> projectQuery = _db.Projects
                .Include(p => p.Team)
                .ThenInclude(t => t.Department);

            if (role == UserRole.DepartmentManager)
                projectQuery = projectQuery.Where(p =>
                    p.Team.DepartmentId == _currentUser.DepartmentId);
            else if (role == UserRole.TeamLead)
                projectQuery = projectQuery.Where(p =>
                    p.TeamId == _currentUser.TeamId);

            var projects = await projectQuery
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            TotalProjects = projects.Count;

            // Görev istatistikleri
            var taskQuery = _db.Tasks.AsQueryable();

            if (role == UserRole.DepartmentManager)
                taskQuery = taskQuery.Where(t =>
                    projects.Select(p => p.Id).Contains(t.ProjectId));
            else if (role == UserRole.TeamLead)
                taskQuery = taskQuery.Where(t =>
                    projects.Select(p => p.Id).Contains(t.ProjectId));

            var tasks = await taskQuery.ToListAsync();

            TotalTasks = tasks.Count;
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Done);
            InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress);
            TodoTasks = tasks.Count(t => t.Status == TaskStatus.Todo);

            CompletionRate = TotalTasks > 0
                ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1)
                : 0;
            CompletionRateDisplay = $"%{CompletionRate}";

            // Kullanıcı sayısı
            if (role == UserRole.Admin)
            {
                TotalUsers = await _db.Users.CountAsync();
                RecentUsers = await _db.Users
                    .Include(u => u.Team)
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToListAsync();
            }
            else if (role == UserRole.DepartmentManager)
            {
                TotalUsers = await _db.Users
                    .CountAsync(u => u.DepartmentId == _currentUser.DepartmentId);
            }
            else if (role == UserRole.TeamLead)
            {
                TotalUsers = await _db.Users
                    .CountAsync(u => u.TeamId == _currentUser.TeamId);
            }

            // Proje özetleri (progress bar için)
            var summaries = new List<ProjectSummary>();
            foreach (var project in projects.Take(6))
            {
                var projectTasks = tasks.Where(t => t.ProjectId == project.Id).ToList();
                var lastCommit = await _db.Commits
                    .Where(c => c.ProjectId == project.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                int progressValue = 0;
                if (lastCommit != null)
                    progressValue = lastCommit.Progress * 10;
                else if (projectTasks.Count > 0)
                    progressValue = (int)Math.Round(
                        (double)projectTasks.Count(t => t.Status == TaskStatus.Done)
                        / projectTasks.Count * 100);

                summaries.Add(new ProjectSummary
                {
                    Id = project.Id,
                    Title = project.Title,
                    TeamName = project.Team.Name,
                    Status = project.Status.ToString(),
                    TotalTasks = projectTasks.Count,
                    CompletedTasks = projectTasks.Count(t => t.Status == TaskStatus.Done),
                    Progress = progressValue,
                    ProgressDisplay = $"%{progressValue}"
                });
            }
            ProjectSummaries = summaries;

            // Son commitler
            RecentCommits = await _db.Commits
                .Include(c => c.User)
                .Include(c => c.Project)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();
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

    // Proje özeti için yardımcı sınıf
    public class ProjectSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int Progress { get; set; }
        public string ProgressDisplay { get; set; } = string.Empty;
    }
}