using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudyHelperMVC.Services;

public class GptService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public GptService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<string> ChatSingleAsync(string userPrompt, string system = "", int? maxOutputTokens = null)
    {
        var client = _httpClientFactory.CreateClient();
        var apiKey = _config["OpenAI:ApiKey"] ?? "";
        var model = _config["OpenAI:Model"] ?? "gpt-4.1-nano";
        var maxTokens = maxOutputTokens ?? int.Parse(_config["OpenAI:MaxOutputTokens"] ?? "700");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = userPrompt }
            },
            max_tokens = maxTokens,
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var resp = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var respText = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return $"OpenAI error: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{respText}";

        using var doc = JsonDocument.Parse(respText);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString() ?? string.Empty;
    }

    // Prompts


    public async Task<string> CompileExercisesSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — интеллектуальный ассистент для студентов. Тебе даётся текст упражнений из учебного материала (например, из PDF). 
Выполни следующие шаги:

1. Разбери каждое упражнение, представленное в тексте.
2. Для каждого упражнения:
   - Сначала перепиши его в удобной нумерованной форме (чтобы сохранить структуру).
   - Создай 1–2 **новых дополнительных задания по той же теме**, но с другими условиями (так, чтобы студент мог попрактиковаться, но тема оставалась та же).
   - Новые задания должны отличаться числами, формулировкой или контекстом, но проверять то же знание/навык.
3. После того как все упражнения и дополнительные задания сгенерированы, выполни блок:
   **Ответы и объяснения:**
   - Для каждого упражнения (и для каждого дополнительного задания) запиши правильный ответ.
   - Под каждым ответом дай краткое объяснение: какое правило, закон, формулу или метод нужно применить для решения.

⚠️ Важно:
- Не придумывай упражнения «не по теме». Всегда держись того же типа, что и в исходном упражнении.
- Формулировка новых заданий должна быть максимально естественной, как в учебниках.
- Объяснения должны быть короткими, но полезными для понимания.

Пример структуры вывода:

---
**Упражнения:**

1. [Исходное упражнение 1]
   - Доп. задание 1
   - Доп. задание 2

2. [Исходное упражнение 2]
   - Доп. задание 1
   - Доп. задание 2

...

**Ответы и объяснения:**

1. Ответ: ...
   Объяснение: ...

1.1 (доп. задание): Ответ: ...
     Объяснение: ...

1.2 (доп. задание): Ответ: ...
     Объяснение: ...

2. Ответ: ...
   Объяснение: ...

...
---
{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь упражнения.", maxOutputTokens: 3000);
    }




    public async Task<string> CompileLectureSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — помощник студента, задача которого — создавать **конспекты из лекций**. Тебе предоставляется выжимка из уже сжатых частей лекции. Выполни следующие шаги:

1. Сократи текст лекции до **максимально компактного конспекта**, сохрани всю важную информацию и ключевые понятия.
2. Структурируй конспект так, чтобы легко было читать и повторять. Можешь использовать списки, заголовки, подпункты.
3. В конце конспекта:
   a) Выпиши все **формулы**, которые встречались в тексте, укажи их **названия или к чему они относятся**, если это возможно.
   b) Выпиши **ключевые слова** из лекции отдельным списком в столбик.
   c) Выпиши нужные вопросы по лекции. 
4. Не добавляй лишнюю информацию — только то, что содержится в лекции.

Пример структуры вывода:

---
**Конспект:**
- Краткая суть темы 1
- Краткая суть темы 2
...

**Формулы:**
1. Формула 1 — Название или назначение
2. Формула 2 — Название или назначение
...

**Ключевые слова:**
- слово1
- слово2
- слово3
...

**Вопросы по лекции:**
- Вопрос 1
- Вопрос 2
- Вопрос 3
...
---
{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь конспекты лекций.", maxOutputTokens: 3000);
    }
}
