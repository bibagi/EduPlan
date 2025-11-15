using Microsoft.EntityFrameworkCore;
using Schedule.Core.Models;

namespace Schedule.Core.Data;

public class ScheduleDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<WeekSchedule> WeekSchedules { get; set; }

    public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Group)
            .WithMany(g => g.Lessons)
            .HasForeignKey(l => l.GroupId);

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Subject)
            .WithMany(s => s.Lessons)
            .HasForeignKey(l => l.SubjectId);

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Teacher)
            .WithMany(t => t.Lessons)
            .HasForeignKey(l => l.TeacherId);

        modelBuilder.Entity<Lesson>()
            .HasOne(l => l.Classroom)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.ClassroomId);

        // WeekSchedule relationships
        modelBuilder.Entity<WeekSchedule>()
            .HasOne(w => w.Group)
            .WithMany()
            .HasForeignKey(w => w.GroupId);

        modelBuilder.Entity<WeekSchedule>()
            .HasOne(w => w.Subject)
            .WithMany()
            .HasForeignKey(w => w.SubjectId);

        modelBuilder.Entity<WeekSchedule>()
            .HasOne(w => w.Teacher)
            .WithMany()
            .HasForeignKey(w => w.TeacherId);

        modelBuilder.Entity<WeekSchedule>()
            .HasOne(w => w.Classroom)
            .WithMany()
            .HasForeignKey(w => w.ClassroomId);
    }
}
