namespace StudyHelperMVC.Models;

public class ExerciseModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;

    // Содержимое (сгенерированные задания + ответы/объяснения)
    public string ExercisesText { get; set; } = string.Empty;

    public int SubjectId { get; set; }
    public SubjectModel Subject { get; set; } = null!;
}
