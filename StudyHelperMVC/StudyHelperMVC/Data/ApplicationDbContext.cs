using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Models;

namespace StudyHelperMVC.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SubjectModel> Subjects => Set<SubjectModel>();
    public DbSet<LectureModel> Lectures => Set<LectureModel>();
    public DbSet<ExerciseModel> Exercises => Set<ExerciseModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SubjectModel>()
            .HasMany(s => s.Lectures)
            .WithOne(l => l.Subject)
            .HasForeignKey(l => l.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SubjectModel>()
            .HasMany(s => s.Exercises)
            .WithOne(e => e.Subject)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
