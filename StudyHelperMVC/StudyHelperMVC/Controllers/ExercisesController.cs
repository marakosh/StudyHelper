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

        // Извлекаем текст из PDF
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

        // Разбиваем на чанки
        var chunks = _chunker.Split(text, 4000);
        var partialExercises = new List<string>();

        // Генерируем упражнения для каждого чанка
        foreach (var chunk in chunks)
        {
            var exercisesChunk = await _gpt.ChatSingleAsync(
                $$"""
Ты — интеллектуальный ассистент для студентов. Извлеки все упражнения из фрагмента текста и подготовь их в удобной нумерованной форме.
Фрагмент:
{{chunk}}
""",
                "Создавай упражнения и задания."
            );
            partialExercises.Add(exercisesChunk);
        }

        // Финальный промпт — объединяем все чанки
        var finalExercises = await _gpt.CompileExercisesSummaryAsync(partialExercises);

        var model = new ExerciseModel
        {
            FileName = file.FileName,
            Content = finalExercises
        };

        return View("Result", model);
    }
}
