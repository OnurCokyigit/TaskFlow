namespace TaskFlow.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#3498db"; // Görsel renk etiketi

        // Navigation
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}