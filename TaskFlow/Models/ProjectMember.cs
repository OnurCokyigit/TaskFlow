namespace TaskFlow.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}