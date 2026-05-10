namespace TaskFlow.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Foreign Keys
        public int DepartmentId { get; set; }
        public int? LeaderId { get; set; }  // Takım Kaptanı

        // Navigation Properties
        public Department Department { get; set; } = null!;
        public User? Leader { get; set; }
        public ICollection<User> Members { get; set; } = new List<User>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}