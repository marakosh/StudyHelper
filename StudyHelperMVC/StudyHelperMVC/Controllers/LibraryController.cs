using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;

namespace StudyHelperMVC.Controllers;

public class LibraryController : Controller
{
    private readonly AppDbContext _db;

    public LibraryController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new LibraryViewModel
        {
            Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync(),
            Lectures = await _db.Lectures.Include(l => l.Subject).OrderByDescending(l => l.Id).ToListAsync(),
            Exercises = await _db.Exercises.Include(e => e.Subject).OrderByDescending(e => e.Id).ToListAsync()
        };
        return View(vm);
    }
}
