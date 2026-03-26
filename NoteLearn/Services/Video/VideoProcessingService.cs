using Microsoft.EntityFrameworkCore;
using NoteLearn.Models;
using NoteLearn.Services.AI;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NoteLearn.Services.Video;

public class VideoProcessingService
{
    private readonly IWebHostEnvironment _env;
    private readonly IEmbeddingService _embeddingService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;

    public VideoProcessingService(
        IWebHostEnvironment env,
        IEmbeddingService embeddingService,
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory)
    {
        _env = env;
        _embeddingService = embeddingService;
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
    }

    private async Task<string> GetColabBaseUrl()
    {
        // Trỏ thẳng vào thư mục gốc của Project
        string filePath = Path.Combine(_env.ContentRootPath, "link.txt");

        if (!File.Exists(filePath))
        {
            // Dự phòng nếu chạy trong thư mục bin
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "link.txt");
        }

        Console.WriteLine($"[Debug FilePath]: Đang tìm link.txt tại: {filePath}");

        if (!File.Exists(filePath)) throw new Exception($"Không tìm thấy file link.txt tại {filePath}");

        var url = await File.ReadAllTextAsync(filePath);
        // Làm sạch link (xóa khoảng trắng, dấu xuyệt cuối, dấu hỏi cuối)
        return url.Trim().TrimEnd('/').TrimEnd('?');
    }

    public async Task ProcessVideoAiAsync(long contentId, string signedVideoUrl, string fileName)
    {
        // TẠO HTTPCLIENT MỚI (Tránh lỗi Disposed Object)
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(10);

        try
        {
            Console.WriteLine($"\n[AI PROCESS START] - ContentId: {contentId}");

            // 1. Làm sạch link từ Supabase (Xử lý dấu ?)
            string cleanUrl = System.Web.HttpUtility.UrlDecode(signedVideoUrl).Trim().TrimEnd('?');
            Console.WriteLine($"[AI Step 1]: Đang tải video từ: {cleanUrl}");

            // 2. Tải bytes video
            var videoBytes = await client.GetByteArrayAsync(cleanUrl);
            Console.WriteLine($"[AI Step 1]: Tải thành công {videoBytes.Length / 1024 / 1024} MB.");

            // 3. Lấy link Colab
            var baseUrl = await GetColabBaseUrl();
            var apiUrl = $"{baseUrl}/api/stt";

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(videoBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("video/mp4");
            form.Add(fileContent, "file", fileName);

            Console.WriteLine($"[AI Step 2]: Đang gửi sang Colab và chờ Whisper xử lý...");
            var response = await client.PostAsync(apiUrl, form);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"Colab Error {response.StatusCode}: {errorDetail}");
            }

            // 4. Nhận dữ liệu JSON
            var whisperData = await response.Content.ReadFromJsonAsync<WhisperResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (whisperData != null)
            {
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("[AI STEP 3]: NỘI DUNG VĂN BẢN TRÍCH XUẤT ĐƯỢC:");
                Console.WriteLine(whisperData.Full_Text);
                Console.WriteLine(new string('=', 50) + "\n");

                // TẠO SCOPE MỚI ĐỂ TRUY CẬP DATABASE (Tránh lỗi Disposed Context)
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<EngLishContext>();

                    var content = await db.Contents.FindAsync(contentId);
                    if (content != null)
                    {
                        // Lưu Full Text
                        content.Description = whisperData.Full_Text;

                        // Lưu Mục lục (Chapters) vào AiMetadata
                        var chaptersJson = JsonSerializer.Serialize(whisperData.Chapters);
                        var metadata = await db.AiMetadata.FirstOrDefaultAsync(m => m.ContentId == contentId);

                        if (metadata != null)
                        {
                            metadata.Summary = chaptersJson;
                        }
                        else
                        {
                            db.AiMetadata.Add(new AiMetadatum { ContentId = contentId, Summary = chaptersJson });
                        }

                        // 5. Cắt nhỏ và tạo Vector Embedding cho từng đoạn (RAG)
                        Console.WriteLine("[AI Step 4]: Đang tạo Vector Embedding...");
                        await SaveToChunks(contentId, whisperData.Chapters, db);

                        // Lưu toàn bộ vào DB
                        await db.SaveChangesAsync();
                        Console.WriteLine($"\n[AI DONE]: Tất cả dữ liệu của Content {contentId} đã hoàn tất!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[!!! AI PROCESS FATAL ERROR !!!]");
            Console.WriteLine($"Lỗi tại ContentId {contentId}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    private async Task SaveToChunks(long contentId, List<ChapterItem> segments, EngLishContext db)
    {
        // Xóa các chunk cũ nếu có
        var oldChunks = db.ContentChunks.Where(c => c.ContentId == contentId);
        db.ContentChunks.RemoveRange(oldChunks);

        // Gom nhóm các đoạn (cửa sổ trượt) để tạo context tốt hơn cho Vector
        int windowSize = 3;
        for (int i = 0; i < segments.Count; i += 2)
        {
            var group = segments.Skip(i).Take(windowSize).ToList();
            if (!group.Any()) break;

            var combinedText = string.Join(" ", group.Select(s => s.Text));

            // Tạo Vector (EmbeddingService thường không bị ảnh hưởng bởi Scope nên dùng trực tiếp)
            var vector = await _embeddingService.EmbedAsync(combinedText);

            Console.WriteLine($"[Chunk Log]: Đã xử lý đoạn {i / 2 + 1} ({group.First().Start}s)");

            var chunk = new ContentChunk
            {
                ContentId = contentId,
                ChunkIndex = i,
                Text = combinedText,
                StartTimeSec = (int)Math.Floor(group.First().Start),
                EndTimeSec = (int)Math.Floor(group.Last().End),
                Embedding = new Pgvector.Vector(vector),
                CreatedAt = DateTime.UtcNow
            };

            db.ContentChunks.Add(chunk);
        }
    }
}

public class WhisperResponse
{
    public string Full_Text { get; set; } = string.Empty;
    public List<ChapterItem> Chapters { get; set; } = new();
}

public class ChapterItem
{
    public double Start { get; set; }
    public double End { get; set; }
    public string Text { get; set; } = string.Empty;
}