using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NoteLearn.Services.AI;

public interface ILlmService
{
    Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
}

public class OpenAiLlmService : ILlmService
{
    // 1. Chuyển sang dùng IHttpClientFactory thay vì HttpClient trực tiếp
    private readonly IHttpClientFactory _httpClientFactory;

    // Lưu ý: Nên đưa Key này vào appsettings.json để bảo mật hơn
    private readonly string _apiKey = "sk-YhSSE352W3l48tEFK2kDRUwApby32RSYKjK9YhTvxkSJ3C5s";
    private const string ApiUrl = "https://api-v2.shopaikey.com/v1/chat/completions";

    public OpenAiLlmService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        // 2. TẠO CLIENT MỚI TỪ FACTORY (Đảm bảo không bao giờ bị Dispose khi chạy ngầm)
        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(100); // Tăng timeout cho AI nếu cần

        var payload = new
        {
            model = "gpt-4o-mini", // Bạn nên dùng model ổn định
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1000,
            temperature = 1.0
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // 3. Sử dụng 'client' vừa tạo để gửi request
        var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API Error: {response.StatusCode} - {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var firstChoice = choices[0];
            if (firstChoice.TryGetProperty("message", out var message))
            {
                if (message.TryGetProperty("content", out var content))
                {
                    return content.GetString() ?? string.Empty;
                }
            }
        }

        throw new Exception($"Cấu trúc JSON trả về không đúng chuẩn OpenAI. Nội dung nhận được: {body}");
    }
}