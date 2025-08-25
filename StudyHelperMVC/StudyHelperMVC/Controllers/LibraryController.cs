using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;

namespace StudyHelperMVC.Controllers
{
    public class LibraryController : Controller
    {
        private readonly AppDbContext _db;

        public LibraryController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var lectures = await _db.Lectures.Include(l => l.Subject).ToListAsync();
            var exercises = await _db.Exercises.Include(e => e.Subject).ToListAsync();

            var model = new LibraryPageModel
            {
                Lectures = lectures,
                Exercises = exercises
            };

            return View(model);
        }

        public async Task<IActionResult> DetailsLecture(int id)
        {
            var lecture = await _db.Lectures.FindAsync(id);
            if (lecture == null) return NotFound();
            return View(lecture);
        }

        public async Task<IActionResult> DetailsExercise(int id)
        {
            var exercise = await _db.Exercises.FindAsync(id);
            if (exercise == null) return NotFound();
            return View(exercise);
        }

        public async Task<IActionResult> DeleteLecture(int id)
        {
            var lecture = await _db.Lectures.FindAsync(id);
            if (lecture != null)
            {
                _db.Lectures.Remove(lecture);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteExercise(int id)
        {
            var exercise = await _db.Exercises.FindAsync(id);
            if (exercise != null)
            {
                _db.Exercises.Remove(exercise);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class LibraryPageModel
    {
        public List<LectureModel> Lectures { get; set; } = new();
        public List<ExerciseModel> Exercises { get; set; } = new();
    }
}
