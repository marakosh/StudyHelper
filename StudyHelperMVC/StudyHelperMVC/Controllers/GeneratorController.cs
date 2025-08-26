using Microsoft.AspNetCore.Mvc;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;

namespace StudyHelperMVC.Controllers;

public class GeneratorController : Controller
{
    private readonly PdfTextExtractor _extractor;
    private readonly Chunker _chunker;
    private readonly GptService _gpt;
    private readonly AppDbContext _db;

    public GeneratorController(PdfTextExtractor extractor, Chunker chunker, GptService gpt, AppDbContext db)
    {
        _extractor = extractor;
        _chunker = chunker;
        _gpt = gpt;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Subjects = _db.Subjects.OrderBy(s => s.Name).ToList();
        return View();
    }

    // Потоковая генерация лекции
    [HttpPost]
    public async Task StreamLecture(IFormFile file)
    {
        Response.ContentType = "text/plain; charset=utf-8";

        if (file == null || file.Length == 0)
        {
            await Response.WriteAsync("[ERROR] Нет файла");
            return;
        }

        string text;
        using (var s = file.OpenReadStream())
            text = _extractor.ExtractText(s);

        var chunks = _chunker.Split(text, 4000);
        var partials = new List<string>();

        // Сначала получаем краткие выжимки по чанкам (не стримим их на клиент, копим локально)
        foreach (var chunk in chunks)
        {
            var summary = await _gpt.ChatSingleAsync(
                $$"""
Ты — ассистент для студентов. Извлеки суть из фрагмента лекции и сделай краткий конспект (5–10 пунктов, термины, оставляй формулы).
Фрагмент:
{{chunk}}
""",
                "Кратко конспектируй учебные тексты.", 800);
            partials.Add(summary);
        }

        // Теперь запускаем финальную компоновку — уже ПОТОКОВО и пишем пользователю по мере генерации
        var finalPrompt = await _gpt.CompileLectureSummaryAsync(partials);
        await foreach (var piece in _gpt.StreamChatAsync(finalPrompt, "Ты структурируешь конспекты лекций.", 2000))
        {
            await Response.WriteAsync(piece);
            await Response.Body.FlushAsync();
        }
    }

    // Потоковая генерация упражнений
    [HttpPost]
    public async Task StreamExercise(IFormFile file)
    {
        Response.ContentType = "text/plain; charset=utf-8";

        if (file == null || file.Length == 0)
        {
            await Response.WriteAsync("[ERROR] Нет файла");
            return;
        }

        string text;
        using (var s = file.OpenReadStream())
            text = _extractor.ExtractText(s);

        var chunks = _chunker.Split(text, 4000);
        var partials = new List<string>();

        foreach (var chunk in chunks)
        {
            var summary = await _gpt.ChatSingleAsync(
                $$"""
Тебе даётся фрагмент с упражнениями. Кратко перепиши задания, оставь суть и обозначения.
Фрагмент:
{{chunk}}
""",
                "Кратко структурируй упражнения.", 800);
            partials.Add(summary);
        }

        var finalPrompt = await _gpt.CompileExercisesSummaryAsync(partials);
        await foreach (var piece in _gpt.StreamChatAsync(finalPrompt, "Ты структурируешь упражнения.", 2000))
        {
            await Response.WriteAsync(piece);
            await Response.Body.FlushAsync();
        }
    }

    // Сохранение в библиотеку
    [HttpPost]
    public async Task<IActionResult> SaveGenerated(string kind, string fileName, string content, int? subjectId)
    {
        if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(content))
            return BadRequest("Пустой контент");

        if (kind.Equals("lecture", StringComparison.OrdinalIgnoreCase))
        {
            var entity = new LectureModel { FileName = fileName ?? "Lecture.pdf", Summary = content, SubjectId = subjectId };
            _db.Lectures.Add(entity);
        }
        else if (kind.Equals("exercise", StringComparison.OrdinalIgnoreCase))
        {
            var entity = new ExerciseModel { FileName = fileName ?? "Exercises.pdf", ExercisesText = content, SubjectId = subjectId };
            _db.Exercises.Add(entity);
        }
        else
        {
            return BadRequest("Wrong kind");
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }
}
