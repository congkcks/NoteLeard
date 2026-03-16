using System.Text.Json;
using System.Text;

namespace NoteLearn.Services.AI;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey = "AIzaSyCXtbb9aBAULE0dNQQJz-lbXlcyrnlZlwI";

    public GeminiEmbeddingService(HttpClient http)
    {
        _http = http
            ?? throw new InvalidOperationException("Gemini API key is missing.");
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={_apiKey}";

        var payload = new
        {
            content = new
            {
                parts = new[]
                {
                    new { text }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var vector = result
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return vector;
    }
}
