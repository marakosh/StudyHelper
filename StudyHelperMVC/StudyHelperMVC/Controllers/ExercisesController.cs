using Microsoft.AspNetCore.Mvc;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;

namespace StudyHelperMVC.Controllers;

public class ExercisesController : Controller
{
    private readonly PdfTextExtractor _extractor;
    private readonly GptService _gpt;
    private readonly Chunker _chunker;

    public ExercisesController(PdfTextExtractor extractor, GptService gpt, Chunker chunker)
    {
        _extractor = extractor;
        _gpt = gpt;
        _chunker = chunker;
    }

    public IActionResult Upload() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Файл не выбран");
            return View();
        }

        string text;
        using (var stream = file.OpenReadStream())
        {
            text = _extractor.ExtractText(stream);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            ModelState.AddModelError("", "Не удалось извлечь текст из PDF");
            return View();
        }

        // Чанкуем
        var chunks = _chunker.Split(text, 4000);
        var partials = new List<string>();

        foreach (var chunk in chunks)
        {
            var part = await _gpt.ChatSingleAsync(
                $$"""
Проанализируй этот блок упражнений/примеров и кратко перечисли виды задач,
которые тут встречаются (по 1–2 предложения на каждый вид).
Блок:
{{chunk}}
""",
                "Ты выделяешь типы задач из методичек."
            );
            partials.Add(part);
        }

        // Финальная сборка — промпт для генерации новых задач + ответы/объяснения
        var exercisesText = await _gpt.CompileExercisesSummaryAsync(partials);

        var model = new ExerciseModel
        {
            FileName = file.FileName,
            ExercisesText = exercisesText
        };

        return View("Result", model);
    }
}
