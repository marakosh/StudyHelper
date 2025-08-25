using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

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

    // Простая генерация (как у тебя было)
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

    // Генерация текста в реальном времени (streaming)
    public async IAsyncEnumerable<string> StreamChatCompletionAsync(
        string userPrompt,
        string system = "",
        int? maxOutputTokens = null)
    {
        var client = _httpClientFactory.CreateClient();
        var apiKey = _config["OpenAI:ApiKey"] ?? "";
        var model = _config["OpenAI:Model"] ?? "gpt-4o-mini";
        var maxTokens = maxOutputTokens ?? int.Parse(_config["OpenAI:MaxOutputTokens"] ?? "700");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

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

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Content = JsonContent.Create(payload);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("data: "))
            {
                var jsonPart = line.Substring("data: ".Length);
                if (jsonPart == "[DONE]") break;

                using var doc = JsonDocument.Parse(jsonPart);
                var delta = doc.RootElement
                               .GetProperty("choices")[0]
                               .GetProperty("delta");

                if (delta.TryGetProperty("content", out var content))
                    yield return content.GetString() ?? "";
            }
        }
    }

    // Пример конспекта лекции
    public async Task<string> CompileLectureSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — помощник студента, задача которого — создавать конспекты из лекций. Текст: 
{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь конспекты лекций.", maxOutputTokens: 3000);
    }

    // Пример упражнений
    public async Task<string> CompileExercisesSummaryAsync(List<string> chunkSummaries)
    {
        var combined = string.Join("\n\n", chunkSummaries);
        var prompt = $$"""
Ты — помощник студента, задача которого — структурировать упражнения. Текст: 
{{combined}}
""";
        return await ChatSingleAsync(prompt, "Ты структурируешь упражнения.", maxOutputTokens: 3000);
    }
}
