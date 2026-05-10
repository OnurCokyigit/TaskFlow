using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Services;
using TaskFlow.Helpers;

namespace TaskFlow.ViewModels
{
    public partial class TeamLeadViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly User _currentUser;

        public TeamLeadViewModel()
        {
            _db = new AppDbContext();
            _currentUser = SessionService.CurrentUser!;
            LoadDataAsync().ConfigureAwait(false);
        }

        // Listeler
        [ObservableProperty] private List<Project> myProjects = new();
        [ObservableProperty] private List<User> teamMembers = new();
        [ObservableProperty] private List<ProjectMember> projectMembers = new();

        // Seçili öğeler
        [ObservableProperty] private Project? selectedProject;
        [ObservableProperty] private User? selectedUserForProject;
        [ObservableProperty] private ProjectMember? selectedProjectMember;

        // Yeni proje formu
        [ObservableProperty] private string newProjectTitle = string.Empty;
        [ObservableProperty] private string newProjectDescription = string.Empty;
        [ObservableProperty] private DateTime newProjectStartDate = DateTime.Today;

        // Mesajlar
        [ObservableProperty] private string projectMessage = string.Empty;
        [ObservableProperty] private string memberMessage = string.Empty;

        // Görev formu
        [ObservableProperty] private string newTaskTitle = string.Empty;
        [ObservableProperty] private string newTaskDescription = string.Empty;
        [ObservableProperty] private TaskPriority newTaskPriority = TaskPriority.Medium;
        [ObservableProperty] private DateTime? newTaskDueDate = DateTime.Today.AddDays(7);
        [ObservableProperty] private User? newTaskAssignedUser;
        [ObservableProperty] private Category? newTaskCategory;
        [ObservableProperty] private ProjectTask? selectedTask;
        [ObservableProperty] private List<Category> categories = new();
        [ObservableProperty] private List<ProjectTask> teamTasks = new();
        [ObservableProperty] private string taskMessage = string.Empty;

        // Dashboard istatistikleri
        [ObservableProperty] private int totalProjects;
        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int completedTasks;
        [ObservableProperty] private int totalMembers;
        [ObservableProperty] private double completionRate;
        [ObservableProperty] private string completionRateDisplay = "0%";
        [ObservableProperty] private List<ProjectSummary> projectSummaries = new();
        [ObservableProperty] private List<Commit> recentCommits = new();

        public List<TaskPriority> TaskPriorities => Enum.GetValues<TaskPriority>().ToList();

        // Seçili proje değişince üyeleri güncelle
        partial void OnSelectedProjectChanged(Project? value)
        {
            if (value != null)
                LoadProjectMembersAsync().ConfigureAwait(false);
            else
                ProjectMembers = new();
        }

        private async Task LoadDataAsync()
        {
            // Sadece kendi takımının projeleri
            MyProjects = await _db.Projects
                .Where(p => p.TeamId == _currentUser.TeamId)
                .Include(p => p.Team)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            // Sadece kendi takımının üyeleri
            TeamMembers = await _db.Users
                .Where(u => u.TeamId == _currentUser.TeamId &&
                            u.Id != _currentUser.Id)
                .ToListAsync();

            // Kategoriler
            Categories = await _db.Categories.ToListAsync();

            // Takımın tüm görevleri
            TeamTasks = await _db.Tasks
                .Where(t => _db.Projects
                    .Where(p => p.TeamId == _currentUser.TeamId)
                    .Select(p => p.Id)
                    .Contains(t.ProjectId))
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // İstatistikler
            TotalProjects = MyProjects.Count;
            TotalMembers = TeamMembers.Count + 1; // +1 kendisi

            var projectIds = MyProjects.Select(p => p.Id).ToList();
            var tasks = await _db.Tasks
                .Where(t => projectIds.Contains(t.ProjectId))
                .ToListAsync();

            TotalTasks = tasks.Count;
            CompletedTasks = tasks.Count(t => t.Status == Models.TaskStatus.Done);
            CompletionRate = TotalTasks > 0
                ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
            CompletionRateDisplay = $"%{CompletionRate}";

            // Proje özetleri
            var summaries = new List<ProjectSummary>();
            foreach (var project in MyProjects)
            {
                var projectTasks = tasks.Where(t => t.ProjectId == project.Id).ToList();
                var lastCommit = await _db.Commits
                    .Where(c => c.ProjectId == project.Id)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefaultAsync();

                int progressValue = lastCommit != null ? lastCommit.Progress * 10
                    : projectTasks.Count > 0
                        ? (int)Math.Round((double)projectTasks
                            .Count(t => t.Status == Models.TaskStatus.Done)
                            / projectTasks.Count * 100) : 0;

                summaries.Add(new ProjectSummary
                {
                    Id = project.Id,
                    Title = project.Title,
                    TeamName = project.Team?.Name ?? "",
                    Status = project.Status.ToString(),
                    TotalTasks = projectTasks.Count,
                    CompletedTasks = projectTasks.Count(t => t.Status == Models.TaskStatus.Done),
                    Progress = progressValue,
                    ProgressDisplay = $"%{progressValue}"
                });
            }
            ProjectSummaries = summaries;

            // Son commitler
            RecentCommits = await _db.Commits
                .Where(c => projectIds.Contains(c.ProjectId))
                .Include(c => c.User)
                .Include(c => c.Project)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync();
        }

        private async Task LoadProjectMembersAsync()
        {
            if (SelectedProject == null) return;

            ProjectMembers = await _db.ProjectMembers
                .Where(pm => pm.ProjectId == SelectedProject.Id)
                .Include(pm => pm.User)
                .ToListAsync();
        }

        // ─── PROJE İŞLEMLERİ ───────────────────────────────

        [RelayCommand]
        private async Task AddProjectAsync()
        {
            ProjectMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewProjectTitle))
            {
                ProjectMessage = "⚠️ Proje adı boş olamaz.";
                return;
            }

            var project = new Project
            {
                Title = NewProjectTitle,
                Description = NewProjectDescription,
                StartDate = NewProjectStartDate,
                TeamId = _currentUser.TeamId!.Value,
                Status = ProjectStatus.Active
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            ProjectMessage = $"✅ '{NewProjectTitle}' projesi oluşturuldu.";
            NewProjectTitle = NewProjectDescription = string.Empty;
            NewProjectStartDate = DateTime.Today;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteProjectAsync()
        {
            if (SelectedProject == null) return;

            var result = MessageBox.Show(
                $"'{SelectedProject?.Title}' projesini silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedProject != null) _db.Projects.Remove(SelectedProject);
            await _db.SaveChangesAsync();
            ProjectMessage = "✅ Proje silindi.";
            SelectedProject = null;
            await LoadDataAsync();
        }

        // ─── ÜYE İŞLEMLERİ ────────────────────────────────

        [RelayCommand]
        private async Task AddMemberToProjectAsync()
        {
            MemberMessage = string.Empty;

            if (SelectedProject == null || SelectedUserForProject == null)
            {
                MemberMessage = "⚠️ Proje ve kullanıcı seçin.";
                return;
            }

            var exists = await _db.ProjectMembers.AnyAsync(pm =>
                pm.ProjectId == SelectedProject.Id &&
                pm.UserId == SelectedUserForProject.Id);

            if (exists)
            {
                MemberMessage = "⚠️ Bu kullanıcı zaten bu projeye üye.";
                return;
            }

            var member = new ProjectMember
            {
                ProjectId = SelectedProject.Id,
                UserId = SelectedUserForProject.Id,
                JoinedAt = DateTime.Now
            };

            _db.ProjectMembers.Add(member);
            await _db.SaveChangesAsync();

            MemberMessage = $"✅ '{SelectedUserForProject.FullName}' projeye eklendi.";
            await LoadProjectMembersAsync();
        }

        [RelayCommand]
        private async Task RemoveMemberFromProjectAsync(ProjectMember member)
        {
            var result = MessageBox.Show(
                $"'{member.User?.FullName}' üyesini projeden çıkarmak istediğinize emin misiniz?",
                "Çıkarma Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();
            MemberMessage = "✅ Üye projeden çıkarıldı.";
            await LoadProjectMembersAsync();
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            TaskMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewTaskTitle) || SelectedProject == null)
            {
                TaskMessage = "⚠️ Görev adı ve proje seçimi zorunludur.";
                return;
            }

            var task = new ProjectTask
            {
                Title = NewTaskTitle,
                Description = NewTaskDescription,
                Priority = NewTaskPriority,
                Status = Models.TaskStatus.Todo,
                DueDate = NewTaskDueDate,
                ProjectId = SelectedProject.Id,
                AssignedUserId = NewTaskAssignedUser?.Id,
                CategoryId = NewTaskCategory?.Id,
                CreatedAt = DateTime.Now
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            TaskMessage = $"✅ '{NewTaskTitle}' görevi eklendi.";
            NewTaskTitle = NewTaskDescription = string.Empty;
            NewTaskAssignedUser = null;
            NewTaskCategory = null;
            NewTaskPriority = TaskPriority.Medium;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteTaskAsync()
        {
            if (SelectedTask == null) return;

            var result = MessageBox.Show(
                $"'{SelectedTask?.Title}' görevini silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedTask != null) _db.Tasks.Remove(SelectedTask);
            await _db.SaveChangesAsync();
            TaskMessage = "✅ Görev silindi.";
            await LoadDataAsync();
        }
    }
}