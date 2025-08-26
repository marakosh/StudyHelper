using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        var apiKey = _config["OpenAI:ApiKey"] ?? "";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    public async Task<string> ChatSingleAsync(string userPrompt, string system = "", int? maxOutputTokens = null)
    {
        var client = CreateClient();
        var model = _config["OpenAI:Model"] ?? "gpt-4.1-nano";
        var maxTokens = maxOutputTokens ?? int.Parse(_config["OpenAI:MaxOutputTokens"] ?? "700");

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
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }

    /// Потоковый ответ как IAsyncEnumerable<string>
    public async IAsyncEnumerable<string> StreamChatAsync(
    string userPrompt,
    string system = "",
    int? maxOutputTokens = null,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var model = _config["OpenAI:Model"] ?? "gpt-4.1-mini";
        var maxTokens = maxOutputTokens ?? int.Parse(_config["OpenAI:MaxOutputTokens"] ?? "1500");

        var payload = new
        {
            model,
            messages = new object[]
            {
            new { role = "system", content = system },
            new { role = "user", content = userPrompt }
            },
            max_tokens = maxTokens,
            temperature = 0.2,
            stream = true
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (!line.StartsWith("data:")) continue;

            var json = line.Substring(5).Trim();
            if (json == "[DONE]") yield break;

            string? piece = null;
            try
            {
                using var chunk = JsonDocument.Parse(json);
                var delta = chunk.RootElement.GetProperty("choices")[0].GetProperty("delta");

                if (delta.TryGetProperty("content", out var token))
                    piece = token.GetString();
            }
            catch
            {
                // Игнорируем битые чанки
            }

            if (!string.IsNullOrEmpty(piece))
                yield return piece!;
        }
    }


    // Компоновка результатов по чанкам
    public async Task<string> CompileLectureSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — помощник студента, задача которого — создавать **конспекты из лекций**. Тебе предоставляется выжимка из уже сжатых частей лекции. Выполни следующие шаги:

1. Сократи текст лекции до **максимально компактного конспекта**, сохрани всю важную информацию и ключевые понятия.
2. Структурируй конспект так, чтобы легко было читать и повторять. Можешь использовать списки, заголовки, подпункты.
3. В конце конспекта:
   a) Выпиши все **формулы** (в LaTeX), по возможности с кратким названием.
   b) Выпиши **ключевые слова**.
   c) Сформулируй **вопросы по лекции**.
4. Не добавляй лишнего — только из лекции.

{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь конспекты лекций.", maxOutputTokens: 3000);
    }

    public async Task<string> CompileExercisesSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — интеллектуальный ассистент по заданиям. На вход — упражнения. Для каждого:
- Перепиши кратко условие (нумерация).
- Сгенерируй 1–2 доп. задания по той же теме (с изменёнными числами/формулировкой).
- Дай ответы и краткие объяснения.
Формулы — в LaTeX.

{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь упражнения.", maxOutputTokens: 3000);
    }
}
