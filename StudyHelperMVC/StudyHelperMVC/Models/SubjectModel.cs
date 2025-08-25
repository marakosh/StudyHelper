using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudyHelperMVC.Models;

public class SubjectModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    // Связь с пользователем
    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser User { get; set; } = null!;

    // Список лекций и упражнений для предмета
    public virtual ICollection<LectureModel> Lectures { get; set; } = new List<LectureModel>();
    public virtual ICollection<ExerciseModel> Exercises { get; set; } = new List<ExerciseModel>();
}
