using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Services;
using TaskStatus = TaskFlow.Models.TaskStatus;

namespace TaskFlow.ViewModels
{
    public partial class ProjectDetailViewModel : ObservableObject
    {
        private readonly AppDbContext _db;
        private readonly User _currentUser;
        public int ProjectId { get; set; }

        public ProjectDetailViewModel()
        {
            _db = new AppDbContext();
            _currentUser = SessionService.CurrentUser!;
        }

        // Proje bilgileri
        [ObservableProperty] private string projectTitle = string.Empty;
        [ObservableProperty] private string projectDescription = string.Empty;
        [ObservableProperty] private string startDateDisplay = string.Empty;
        [ObservableProperty] private string teamName = string.Empty;
        [ObservableProperty] private string leaderName = string.Empty;
        [ObservableProperty] private string projectStatus = string.Empty;

        // Görevler
        [ObservableProperty] private List<ProjectTask> todoTasks = new();
        [ObservableProperty] private List<ProjectTask> inProgressTasks = new();
        [ObservableProperty] private List<ProjectTask> doneTasks = new();
        [ObservableProperty] private List<ProjectTask> allTasks = new();

        // Üyeler
        [ObservableProperty] private List<ProjectMember> members = new();

        // Commitler
        [ObservableProperty] private List<Commit> commits = new();

        // Yeni commit formu
        [ObservableProperty] private string newCommitMessage = string.Empty;
        [ObservableProperty] private int selectedProgress = 1;
        [ObservableProperty] private string commitMessage = string.Empty;

        // Yetki kontrolleri
        [ObservableProperty] private bool canManageTasks = false;

        // Sekme
        [ObservableProperty] private int selectedTabIndex = 0;

        // Progress sayı kutuları için seçili değer
        private int _progressValue = 1;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                SetProperty(ref _progressValue, value);
                OnPropertyChanged(nameof(ProgressDisplay));
            }
        }

        public string ProgressDisplay => $"%{ProgressValue * 10} Tamamlandı";

        public async Task InitializeAsync(int projectId)
        {
            ProjectId = projectId;
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var project = await _db.Projects
                .Include(p => p.Team)
                    .ThenInclude(t => t.Leader)
                .FirstOrDefaultAsync(p => p.Id == ProjectId);

            if (project == null) return;

            ProjectTitle = project.Title;
            ProjectDescription = project.Description;
            StartDateDisplay = project.StartDate.ToString("dd MMMM yyyy");
            TeamName = project.Team.Name;
            LeaderName = project.Team.Leader?.FullName ?? "—";
            ProjectStatus = project.Status.ToString();

            // Yetki: Admin, DeptManager, TeamLead görev yönetebilir
            CanManageTasks = _currentUser.Role != UserRole.Employee;

            // Üyeler
            Members = await _db.ProjectMembers
                .Where(pm => pm.ProjectId == ProjectId)
                .Include(pm => pm.User)
                .ToListAsync();

            // Görevler
            var tasks = await _db.Tasks
                .Where(t => t.ProjectId == ProjectId)
                .Include(t => t.AssignedUser)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            AllTasks = tasks;
            TodoTasks = tasks.Where(t => t.Status == TaskStatus.Todo).ToList();
            InProgressTasks = tasks.Where(t => t.Status == TaskStatus.InProgress).ToList();
            DoneTasks = tasks.Where(t => t.Status == TaskStatus.Done).ToList();

            // Commitler
            Commits = await _db.Commits
                .Where(c => c.ProjectId == ProjectId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Progress sayı kutularından seçim
        [RelayCommand]
        private void SelectProgress(string value)
        {
            if (int.TryParse(value, out int parsed))
                ProgressValue = parsed;
        }

        // Commit gönder
        [RelayCommand]
        public async Task SubmitCommitAsync()
        {
            CommitMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewCommitMessage))
            {
                CommitMessage = "⚠️ Commit mesajı boş olamaz.";
                return;
            }

            var commit = new Commit
            {
                ProjectId = ProjectId,
                UserId = _currentUser.Id,
                Message = NewCommitMessage,
                Progress = ProgressValue,
                CreatedAt = DateTime.Now
            };

            _db.Commits.Add(commit);
            await _db.SaveChangesAsync();

            CommitMessage = "✅ Commit gönderildi.";
            NewCommitMessage = string.Empty;
            ProgressValue = 1;
            await LoadDataAsync();
        }

        // Göreve tıklanınca durum değiştir (Employee için)
        [RelayCommand]
        private async Task ToggleTaskStatusAsync(ProjectTask task)
        {
            if (_currentUser.Role == UserRole.Employee)
            {
                // Sadece atandığı görevleri değiştirebilir
                if (task.AssignedUserId != _currentUser.Id) return;
            }

            task.Status = task.Status switch
            {
                TaskStatus.Todo => TaskStatus.InProgress,
                TaskStatus.InProgress => TaskStatus.Done,
                TaskStatus.Done => TaskStatus.Todo,
                _ => TaskStatus.Todo
            };

            await _db.SaveChangesAsync();
            await LoadDataAsync();
        }
        public async Task ChangeTaskStatusAsync(ProjectTask task, TaskStatus newStatus)
        {
            var dbTask = await _db.Tasks.FindAsync(task.Id);
            if (dbTask == null) return;

            dbTask.Status = newStatus;
            await _db.SaveChangesAsync();
            await LoadDataAsync();
        }
    }
}