namespace TaskFlow.Models
{
    public class Commit
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Progress { get; set; }  // 1-10 arası
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}