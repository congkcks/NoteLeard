using System.Text;
using System.Text.Json;

namespace NoteLearn.Services.AI;

public interface ILlmService
{
    Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
}

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _http;

    public OllamaLlmService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        var payload = new
        {
            model = "qwen2.5:7b",
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(payload);

        var response = await _http.PostAsync(
            "http://localhost:11434/api/generate",
            new StringContent(json, Encoding.UTF8, "application/json"),
            ct
        );

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        return result
            .GetProperty("response")
            .GetString() ?? "";
    }
}