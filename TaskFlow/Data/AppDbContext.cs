using Microsoft.EntityFrameworkCore;
using System.IO;
using TaskFlow.Models;

namespace TaskFlow.Data
{
    public class AppDbContext : DbContext
    {
        // Tablolar
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Commit> Commits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "taskflow.db"
            );
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Team → Leader ilişkisi (döngüsel ilişkiyi önlemek için)
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Leader)
                .WithMany()
                .HasForeignKey(t => t.LeaderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Team → Members ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(u => u.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // ProjectMember → bileşik unique kural
            // Aynı kullanıcı aynı projeye iki kez eklenemez
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();

            // UserRole enum → string olarak kaydet (okunabilirlik için)
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // ProjectStatus enum → string
            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // TaskPriority enum → string
            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            // TaskStatus enum → string
            modelBuilder.Entity<ProjectTask>()
                .Property(t => t.Status)
                .HasConversion<string>();
        }
    }
}