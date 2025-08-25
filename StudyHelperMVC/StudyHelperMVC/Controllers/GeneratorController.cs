using Microsoft.AspNetCore.Mvc;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;

namespace StudyHelperMVC.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly GptService _gpt;
        private readonly Chunker _chunker;
        private readonly AppDbContext _db;

        public GeneratorController(GptService gpt, Chunker chunker, AppDbContext db)
        {
            _gpt = gpt;
            _chunker = chunker;
            _db = db;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Text is empty");

            string result;

            if (request.Type == "lecture")
            {
                var chunks = _chunker.Split(request.Text, 4000);
                var partialSummaries = new List<string>();
                foreach (var chunk in chunks)
                    partialSummaries.Add(await _gpt.ChatSingleAsync(
                        $$"""
                        Ты — ассистент студента. Извлеки суть из фрагмента лекции и сделай краткий конспект.
                        Требования:
                        - Список из 5–10 пунктов
                        - Ключевые термины и определения
                        - Формулы оставляй в тексте, если есть
                        Фрагмент:
                        {{chunk}}
                        """));
                result = await _gpt.CompileLectureSummaryAsync(partialSummaries);
            }
            else if (request.Type == "exercise")
            {
                result = await _gpt.CompileExercisesSummaryAsync(new List<string> { request.Text });
            }
            else
            {
                return BadRequest("Unknown type");
            }

            return Json(new { result });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content) || string.IsNullOrWhiteSpace(request.FileName))
                return BadRequest("Invalid data");

            if (request.Type == "lecture")
            {
                _db.Lectures.Add(new LectureModel
                {
                    FileName = request.FileName,
                    Summary = request.Content,
                    SubjectId = request.SubjectId
                });
            }
            else if (request.Type == "exercise")
            {
                _db.Exercises.Add(new ExerciseModel
                {
                    FileName = request.FileName,
                    ExercisesText = request.Content,
                    SubjectId = request.SubjectId
                });
            }
            else
            {
                return BadRequest("Unknown type");
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Saved successfully" });
        }
    }

    public class GenerateRequest
    {
        public string Type { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class SaveRequest
    {
        public string Type { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Content { get; set; } = "";
        public int SubjectId { get; set; }
    }
}
