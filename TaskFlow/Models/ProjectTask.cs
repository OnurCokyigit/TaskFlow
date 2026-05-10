namespace TaskFlow.Models
{
    public enum TaskPriority
    {
        Low,    // Düşük
        Medium, // Orta
        High,   // Yüksek
        Critical // Kritik
    }

    public enum TaskStatus
    {
        Todo,       // Yapılacak
        InProgress, // Devam Ediyor
        Done,       // Tamamlandı
        Cancelled   // İptal
    }

    public class ProjectTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }
        public int? CategoryId { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;
        public User? AssignedUser { get; set; }
        public Category? Category { get; set; }
    }
}