using Microsoft.AspNetCore.Identity;

namespace StudyHelperMVC.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public ICollection<SubjectModel> Subjects { get; set; } = new List<SubjectModel>();
}
