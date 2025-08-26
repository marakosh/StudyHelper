namespace StudyHelperMVC.Models;

public class LibraryViewModel
{
    public List<SubjectModel> Subjects { get; set; } = new();
    public List<LectureModel> Lectures { get; set; } = new();
    public List<ExerciseModel> Exercises { get; set; } = new();
}
