using System.Text.Json;
using System.Text;

namespace NoteLearn.Services.AI;

public class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;

    public OllamaEmbeddingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var url = "http://localhost:11434/api/embeddings";

        var payload = new
        {
            model = "nomic-embed-text:latest",
            prompt = text
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var vector = result
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return vector;
    }
}