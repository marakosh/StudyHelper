namespace StudyHelperMVC.Models;

public class LectureModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public int? SubjectId { get; set; }
    public SubjectModel? Subject { get; set; }
}
