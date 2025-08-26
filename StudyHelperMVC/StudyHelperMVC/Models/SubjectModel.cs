namespace StudyHelperMVC.Models;

public class SubjectModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Опционально, пока нет пользователей
    public string? UserId { get; set; }

    public List<LectureModel> Lectures { get; set; } = new();
    public List<ExerciseModel> Exercises { get; set; } = new();
}
