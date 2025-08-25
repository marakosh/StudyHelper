using Microsoft.AspNetCore.Mvc;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;

namespace StudyHelperMVC.Controllers;

public class LectureController : Controller
{
    private readonly PdfTextExtractor _extractor;
    private readonly GptService _gpt;
    private readonly Chunker _chunker;

    public LectureController(PdfTextExtractor extractor, GptService gpt, Chunker chunker)
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

        var chunks = _chunker.Split(text, 4000);

        var partialSummaries = new List<string>();

        foreach (var chunk in chunks)
        {
            var summary = await _gpt.ChatSingleAsync(
                $$"""
Ты — ассистент для студентов. Извлеки суть из фрагмента лекции и сделай краткий конспект.
Требования:
- Список из 5–10 пунктов
- Ключевые термины и определения
- Формулы оставляй в тексте, если есть

Фрагмент:
{{chunk}}
""",
                "Кратко конспектируй учебные тексты."
            );
            partialSummaries.Add(summary);
        }

        var finalSummary = await _gpt.CompileLectureSummaryAsync(partialSummaries);

        var model = new LectureModel
        {
            FileName = file.FileName,
            Summary = finalSummary
        };

        return View("Result", model);
    }
}
