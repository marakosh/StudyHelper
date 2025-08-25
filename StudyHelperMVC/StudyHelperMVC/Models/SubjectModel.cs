namespace StudyHelperMVC.Models;

public class SubjectModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Владение
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ICollection<LectureModel> Lectures { get; set; } = new List<LectureModel>();
    public ICollection<ExerciseModel> Exercises { get; set; } = new List<ExerciseModel>();
}
