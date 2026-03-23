using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NoteLearn.Services.AI;

/// <summary>
/// Interface định nghĩa dịch vụ LLM
/// </summary>
public interface ILlmService
{
    Task<string> GenerateAsync(string prompt, CancellationToken ct = default);
}

/// <summary>
/// Triển khai dịch vụ LLM theo chuẩn OpenAI (Tương thích với SHOPAIKEY)
/// </summary>
public class OpenAiLlmService : ILlmService
{
    private readonly HttpClient _http;

    // ⚠️ CẢNH BÁO: API Key này đã bị lộ trong lịch sử chat, bạn nên tạo key mới!
    private readonly string _apiKey = "sk-YhSSE352W3l48tEFK2kDRUwApby32RSYKjK9YhTvxkSJ3C5s";

    // Sử dụng Endpoint chuẩn OpenAI Chat Completions
    private const string ApiUrl = "https://api-v2.shopaikey.com/v1/chat/completions";

    public OpenAiLlmService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
    {
        // 1. Chuẩn bị Payload theo cấu trúc messages (giống hệt code Python/JS của bạn)
        var payload = new
        {
            model = "gpt-4o", // Hoặc gpt-4o-mini, gpt-5-mini tùy gói của bạn
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

        // 2. Gửi request
        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        // Debug log (Có thể xóa khi chạy production)
        Console.WriteLine("=== RAW BODY FROM API ===");
        Console.WriteLine(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API Error: {response.StatusCode} - {body}");
        }

        // 3. Parse JSON theo chuẩn: choices[0].message.content
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Kiểm tra từng cấp để tránh lỗi NullReferenceException
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

        // Nếu không tìm thấy cấu trúc chuẩn, quăng lỗi chi tiết
        throw new Exception($"Cấu trúc JSON trả về không đúng chuẩn OpenAI. Nội dung nhận được: {body}");
    }
}