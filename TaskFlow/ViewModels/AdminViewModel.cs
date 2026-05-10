using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Services;

namespace TaskFlow.ViewModels
{
    public partial class AdminViewModel : ObservableObject
    {
        private readonly AppDbContext _db;

        public AdminViewModel()
        {
            _db = new AppDbContext();
            LoadDataAsync().ConfigureAwait(false);
        }

        // Listeler
        [ObservableProperty] private List<User> users = new();
        [ObservableProperty] private List<Department> departments = new();
        [ObservableProperty] private List<Team> teams = new();
        [ObservableProperty] private List<Project> projects = new();

        // Seçili öğeler
        [ObservableProperty] private User? selectedUser;
        [ObservableProperty] private Department? selectedDepartment;
        [ObservableProperty] private Team? selectedTeam;
        [ObservableProperty] private Project? selectedProject;

        // Yeni kullanıcı formu
        [ObservableProperty] private string newUsername = string.Empty;
        [ObservableProperty] private string newFullName = string.Empty;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private UserRole newUserRole = UserRole.Employee;
        [ObservableProperty] private Department? newUserDepartment;
        [ObservableProperty] private Team? newUserTeam;

        // Yeni departman formu
        [ObservableProperty] private string newDepartmentName = string.Empty;

        // Yeni takım formu
        [ObservableProperty] private string newTeamName = string.Empty;
        [ObservableProperty] private Department? newTeamDepartment;

        // Yeni proje formu
        [ObservableProperty] private string newProjectTitle = string.Empty;
        [ObservableProperty] private string newProjectDescription = string.Empty;
        [ObservableProperty] private Team? newProjectTeam;
        [ObservableProperty] private DateTime newProjectStartDate = DateTime.Today;

        // Hata / Başarı mesajları
        [ObservableProperty] private string userMessage = string.Empty;
        [ObservableProperty] private string departmentMessage = string.Empty;
        [ObservableProperty] private string teamMessage = string.Empty;
        [ObservableProperty] private string projectMessage = string.Empty;

        // Proje üye yönetimi
        [ObservableProperty] private Project? selectedProjectForMember;
        [ObservableProperty] private User? selectedUserForProject;
        [ObservableProperty] private List<ProjectMember> projectMembers = new();
        [ObservableProperty] private string memberMessage = string.Empty;

        // Enum listesi (ComboBox için)
        public List<UserRole> UserRoles => Enum.GetValues<UserRole>().ToList();

        // Görev formu
        [ObservableProperty] private string newTaskTitle = string.Empty;
        [ObservableProperty] private string newTaskDescription = string.Empty;
        [ObservableProperty] private TaskPriority newTaskPriority = TaskPriority.Medium;
        [ObservableProperty] private DateTime? newTaskDueDate = DateTime.Today.AddDays(7);
        [ObservableProperty] private Project? newTaskProject;
        [ObservableProperty] private User? newTaskAssignedUser;
        [ObservableProperty] private string taskMessage = string.Empty;

        // Kategoriler
        [ObservableProperty] private List<Category> categories = new();
        [ObservableProperty] private Category? newTaskCategory;

        [ObservableProperty] private List<ProjectTask> allTasks = new();
        [ObservableProperty] private ProjectTask? selectedTask;

        public List<TaskPriority> TaskPriorities => Enum.GetValues<TaskPriority>().ToList();

        private async Task LoadDataAsync()
        {
            Users = await _db.Users
                .Include(u => u.Department)
                .Include(u => u.Team)
                .ToListAsync();

            Departments = await _db.Departments.ToListAsync();

            Teams = await _db.Teams
                .Include(t => t.Department)
                .Include(t => t.Leader)
                .ToListAsync();

            Projects = await _db.Projects
                .Include(p => p.Team)
                .ThenInclude(t => t.Department)
                .ToListAsync();

            Categories = await _db.Categories.ToListAsync();

            AllTasks = await _db.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.Category)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            // Proje üyelerini de yükle
            if (SelectedProjectForMember != null)
            {
                ProjectMembers = await _db.ProjectMembers
                    .Where(pm => pm.ProjectId == SelectedProjectForMember.Id)
                    .Include(pm => pm.User)
                    .ToListAsync();
            }
        }

        // ─── KULLANICI İŞLEMLERİ ───────────────────────────

        [RelayCommand]
        private async Task AddUserAsync()
        {
            UserMessage = string.Empty;

            // Temel alan kontrolü
            if (string.IsNullOrWhiteSpace(NewUsername) ||
                string.IsNullOrWhiteSpace(NewFullName) ||
                string.IsNullOrWhiteSpace(NewPassword))
            {
                UserMessage = "⚠️ Kullanıcı adı, ad soyad ve şifre boş olamaz.";
                return;
            }

            // Role göre departman/takım zorunluluğu
            if (NewUserRole == UserRole.Employee || NewUserRole == UserRole.TeamLead)
            {
                if (NewUserDepartment == null)
                {
                    UserMessage = "⚠️ Employee ve TeamLead için departman seçimi zorunludur.";
                    return;
                }
                if (NewUserTeam == null)
                {
                    UserMessage = "⚠️ Employee ve TeamLead için takım seçimi zorunludur.";
                    return;
                }
            }

            if (NewUserRole == UserRole.DepartmentManager && NewUserDepartment == null)
            {
                UserMessage = "⚠️ Departman Yöneticisi için departman seçimi zorunludur.";
                return;
            }

            // Kullanıcı adı minimum uzunluk
            if (NewUsername.Length < 3)
            {
                UserMessage = "⚠️ Kullanıcı adı en az 3 karakter olmalıdır.";
                return;
            }

            // Şifre minimum uzunluk
            if (NewPassword.Length < 6)
            {
                UserMessage = "⚠️ Şifre en az 6 karakter olmalıdır.";
                return;
            }

            var exists = await _db.Users
                .AnyAsync(u => u.Username == NewUsername);

            if (exists)
            {
                UserMessage = "⚠️ Bu kullanıcı adı zaten alınmış.";
                return;
            }

            var passwordService = new PasswordService();
            var user = new User
            {
                Username = NewUsername,
                FullName = NewFullName,
                PasswordHash = passwordService.HashPassword(NewPassword),
                Role = NewUserRole,
                DepartmentId = NewUserDepartment?.Id,
                TeamId = NewUserTeam?.Id
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            UserMessage = $"✅ '{NewFullName}' başarıyla eklendi.";
            NewUsername = NewFullName = NewPassword = string.Empty;
            NewUserDepartment = null;
            NewUserTeam = null;
            NewUserRole = UserRole.Employee;

            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            if (SelectedUser.Role == UserRole.Admin)
            {
                UserMessage = "⚠️ Admin kullanıcısı silinemez.";
                return;
            }

            var result = MessageBox.Show(
                $"'{SelectedUser?.FullName}' adlı kullanıcıyı silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedUser != null) _db.Users.Remove(SelectedUser);
            await _db.SaveChangesAsync();
            UserMessage = "✅ Kullanıcı silindi.";
            await LoadDataAsync();
        }

        // ─── DEPARTMAN İŞLEMLERİ ───────────────────────────

        [RelayCommand]
        private async Task AddDepartmentAsync()
        {
            DepartmentMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewDepartmentName))
            {
                DepartmentMessage = "⚠️ Departman adı boş olamaz.";
                return;
            }

            var dept = new Department { Name = NewDepartmentName };
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();

            DepartmentMessage = $"✅ '{NewDepartmentName}' departmanı eklendi.";
            NewDepartmentName = string.Empty;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteDepartmentAsync()
        {
            if (SelectedDepartment == null) return;

            var result = MessageBox.Show(
                $"'{SelectedDepartment?.Name}' departmanını silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedDepartment != null) _db.Departments.Remove(SelectedDepartment);
            await _db.SaveChangesAsync();
            DepartmentMessage = "✅ Departman silindi.";
            await LoadDataAsync();
        }

        // ─── TAKIM İŞLEMLERİ ───────────────────────────────

        [RelayCommand]
        private async Task AddTeamAsync()
        {
            TeamMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewTeamName) || NewTeamDepartment == null)
            {
                TeamMessage = "⚠️ Takım adı ve departman seçin.";
                return;
            }

            var team = new Team
            {
                Name = NewTeamName,
                DepartmentId = NewTeamDepartment.Id
            };

            _db.Teams.Add(team);
            await _db.SaveChangesAsync();

            TeamMessage = $"✅ '{NewTeamName}' takımı eklendi.";
            NewTeamName = string.Empty;
            NewTeamDepartment = null;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteTeamAsync()
        {
            if (SelectedTeam == null) return;

            var result = MessageBox.Show(
                $"'{SelectedTeam?.Name}' takımını silmek istediğinize emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            if (SelectedTeam != null) _db.Teams.Remove(SelectedTeam);
            await _db.SaveChangesAsync();
            TeamMessage = "✅ Takım silindi.";
            await LoadDataAsync();
        }

        // ─── PROJE İŞLEMLERİ ───────────────────────────────

        [RelayCommand]
        private async Task AddProjectAsync()
        {
            ProjectMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewProjectTitle) || NewProjectTeam == null)
            {
                ProjectMessage = "⚠️ Proje adı ve takım seçin.";
                return;
            }

            var project = new Project
            {
                Title = NewProjectTitle,
                Description = NewProjectDescription,
                StartDate = NewProjectStartDate,
                TeamId = NewProjectTeam.Id,
                Status = ProjectStatus.Active
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            ProjectMessage = $"✅ '{NewProjectTitle}' projesi eklendi.";
            NewProjectTitle = NewProjectDescription = string.Empty;
            NewProjectTeam = null;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            TaskMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewTaskTitle) || NewTaskProject == null)
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
                ProjectId = NewTaskProject.Id,
                AssignedUserId = NewTaskAssignedUser?.Id,
                CategoryId = NewTaskCategory?.Id,
                CreatedAt = DateTime.Now
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            TaskMessage = $"✅ '{NewTaskTitle}' görevi eklendi.";
            NewTaskTitle = NewTaskDescription = string.Empty;
            NewTaskProject = null;
            NewTaskAssignedUser = null;
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
            await LoadDataAsync();
        }

        // Proje seçilince üyeleri güncelle
        partial void OnSelectedProjectForMemberChanged(Project? value)
        {
            if (value != null)
                LoadProjectMembersAsync().ConfigureAwait(false);
        }

        private async Task LoadProjectMembersAsync()
        {
            if (SelectedProjectForMember == null) return;

            ProjectMembers = await _db.ProjectMembers
                .Where(pm => pm.ProjectId == SelectedProjectForMember.Id)
                .Include(pm => pm.User)
                .ToListAsync();
        }

        [RelayCommand]
        private async Task AddMemberToProjectAsync()
        {
            MemberMessage = string.Empty;

            if (SelectedProjectForMember == null || SelectedUserForProject == null)
            {
                MemberMessage = "⚠️ Proje ve kullanıcı seçin.";
                return;
            }

            // Zaten üye mi?
            var exists = await _db.ProjectMembers.AnyAsync(pm =>
                pm.ProjectId == SelectedProjectForMember.Id &&
                pm.UserId == SelectedUserForProject.Id);

            if (exists)
            {
                MemberMessage = "⚠️ Bu kullanıcı zaten bu projeye üye.";
                return;
            }

            var member = new ProjectMember
            {
                ProjectId = SelectedProjectForMember.Id,
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
            _db.ProjectMembers.Remove(member);
            await _db.SaveChangesAsync();
            MemberMessage = "✅ Üye projeden çıkarıldı.";
            await LoadProjectMembersAsync();
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