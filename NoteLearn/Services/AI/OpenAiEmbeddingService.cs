using System.Text.Json;
using System.Text;

namespace NoteLearn.Services.AI;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey = "sk-YhSSE352W3l48tEFK2kDRUwApby32RSYKjK9YhTvxkSJ3C5s";

    public OpenAIEmbeddingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        var url = "https://api-v2.shopaikey.com/v1/embeddings";

        var payload = new
        {
            model = "text-embedding-3-small",
            input = text
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        var vector = json
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();

        return vector;
    }
}