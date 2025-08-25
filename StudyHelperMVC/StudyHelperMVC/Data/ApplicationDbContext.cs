using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Models;

namespace StudyHelperMVC.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SubjectModel> Subjects => Set<SubjectModel>();
    public DbSet<LectureModel> Lectures => Set<LectureModel>();
    public DbSet<ExerciseModel> Exercises => Set<ExerciseModel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SubjectModel>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);

            e.HasOne(s => s.User)
             .WithMany(u => u.Subjects)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LectureModel>(e =>
        {
            e.Property(x => x.FileName).IsRequired().HasMaxLength(300);
            e.Property(x => x.Summary).IsRequired();

            e.HasOne(l => l.Subject)
             .WithMany(s => s.Lectures)
             .HasForeignKey(l => l.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ExerciseModel>(e =>
        {
            e.Property(x => x.FileName).IsRequired().HasMaxLength(300);
            e.Property(x => x.ExercisesText).IsRequired();

            e.HasOne(x => x.Subject)
             .WithMany(s => s.Exercises)
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
