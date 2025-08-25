using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace StudyHelperMVC.Models;

public class ApplicationUser : IdentityUser
{
    // Пользователь может иметь несколько предметов
    public virtual ICollection<SubjectModel> Subjects { get; set; } = new List<SubjectModel>();
}
