namespace TaskFlow.Models
{
    public enum UserRole
    {
        Employee,           // Çalışan
        TeamLead,           // Takım Kaptanı
        DepartmentManager,  // Departman Yöneticisi
        Admin               // Yönetici
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // Foreign Keys
        public int? DepartmentId { get; set; }
        public int? TeamId { get; set; }

        // Navigation Properties
        public Department? Department { get; set; }
        public Team? Team { get; set; }
        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<Commit> Commits { get; set; } = new List<Commit>();
    }
}