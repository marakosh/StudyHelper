using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Models;

namespace StudyHelperMVC.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<SubjectModel> Subjects { get; set; } = null!;
    public DbSet<LectureModel> Lectures { get; set; } = null!;
    public DbSet<ExerciseModel> Exercises { get; set; } = null!;
}
