namespace TaskFlow.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Team> Teams { get; set; } = new List<Team>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}