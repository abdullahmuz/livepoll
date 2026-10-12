using LivePoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LivePoll.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<Vote> Votes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // column sizes (required so SQL Server can index these columns)
        modelBuilder.Entity<User>().Property(u => u.UserName).HasMaxLength(50);
        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        modelBuilder.Entity<Session>().Property(s => s.JoinCode).HasMaxLength(10);
        modelBuilder.Entity<Vote>().Property(v => v.ParticipantId).HasMaxLength(64);

        // unique indexes
        modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Session>().HasIndex(s => s.JoinCode).IsUnique();

        // one vote per participant per question, enforced by the database
        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.ParticipantId, v.QuestionId })
            .IsUnique();

        // Vote -> Question must NOT cascade (explained below)
        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Question)
            .WithMany()
            .HasForeignKey(v => v.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}