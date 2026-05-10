namespace TaskFlow.Models
{
    public enum ProjectStatus
    {
        Active,     // Devam Ediyor
        OnHold,     // Beklemede
        Completed,  // Tamamlandı
        Cancelled   // İptal Edildi
    }

    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }

        // Foreign Key
        public int TeamId { get; set; }

        // Navigation Properties
        public Team Team { get; set; } = null!;
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public ICollection<Commit> Commits { get; set; } = new List<Commit>();
    }
}