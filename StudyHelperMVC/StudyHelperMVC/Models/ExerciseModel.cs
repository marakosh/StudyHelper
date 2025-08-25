using System.ComponentModel.DataAnnotations;

namespace StudyHelperMVC.Models;

public class ExerciseModel
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    // Связь с предметом
    [Required]
    public int SubjectId { get; set; }
    public virtual SubjectModel Subject { get; set; } = null!;
}
