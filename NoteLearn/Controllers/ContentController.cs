using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteLearn.Dtos;
using NoteLearn.Models;
using NoteLearn.Services.Background;
using NoteLearn.Services.Ingest;
using NoteLearn.Services.Storage;
using NoteLearn.Services.Video;
namespace NoteLearn.Controllers;
public class ContentController : ControllerBase
{
    private readonly EngLishContext _db;
    private readonly FileStorageService _storage;
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    public ContentController(EngLishContext db, FileStorageService storage,IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory)
    {
        _db = db;
        _storage = storage;
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    [HttpPost("upload-pdf")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPdf([FromForm] UploadPdfRequest request)
    {
        var file = request.File;
        var userId = request.UserId;
        if (file == null || file.Length == 0) return BadRequest("No file.");
        if (file.ContentType != "application/pdf") return BadRequest("Only PDF supported.");
        var content = new Content
        {
            UserId = userId,
            Title = file.FileName,
            Type = "pdf",
            CreatedAt = DateTime.Now
        };
        var url = await _storage.UploadPdfAsync(file, userId, content.Id);
        content.FileUrl = url;
        _db.Contents.Add(content);
        await _db.SaveChangesAsync();
        return Ok(new
        {
            content.Id,
            content.Title,
            content.Type,
            content.FileUrl,
            message = "Uploaded. PDF is being processed in background."
        });
    }
    [HttpPost("upload-video")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadVideo(
    [FromForm] UploadVideoRequest request,
    [FromServices] VideoProcessingService videoAiService)
    {
        var file = request.File;
        var userId = request.UserId;

        if (file == null || file.Length == 0)
            return BadRequest("Không có file video được tải lên.");

        var allowedContentTypes = new[] { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo" };
        if (!allowedContentTypes.Contains(file.ContentType))
            return BadRequest("Chỉ hỗ trợ định dạng video (MP4, MPEG, MOV, AVI).");

        var content = new Content
        {
            UserId = userId,
            Title = file.FileName,
            Type = "video",
            CreatedAt = DateTime.Now
        };

        try
        {
            // 1. Lưu bản ghi để lấy ID
            _db.Contents.Add(content);
            await _db.SaveChangesAsync();

            // 2. Upload video lên Supabase Storage
            var path = await _storage.UploadVideoAsync(file, userId, content.Id);

            // 3. Cập nhật lại Path vào DB
            content.FileUrl = path;
            await _db.SaveChangesAsync();

            // --- BƯỚC QUAN TRỌNG: TẠO SIGNED URL ---
            // Lấy link có chữ ký để Service AI có quyền tải file về
            var signedUrl = await _storage.GetSignedVideoUrl(path);
            var fileName = file.FileName;

            // 4. GỌI AI XỬ LÝ NGẦM
            _ = Task.Run(async () =>
            {
                try
                {
                    // Truyền contentId, signedUrl và fileName vào Service
                    await videoAiService.ProcessVideoAiAsync(content.Id, signedUrl, fileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AI Error tại Content {content.Id}]: {ex.Message}");
                }
            });

            return Ok(new
            {
                content.Id,
                content.Title,
                content.Type,
                content.FileUrl,
                message = "Video đã tải lên thành công. Hệ thống AI đang bắt đầu xử lý transcript ngầm."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
    }


    [HttpPost("add-youtube")]
    public async Task<IActionResult> AddYoutube(string title, string url, int UserId)
    {
        var content = new Content
        {
            UserId = UserId,
            Title = title,
            Type = "youtube",
            YoutubeUrl = url,
            CreatedAt = DateTime.Now
        };

        _db.Contents.Add(content);
        await _db.SaveChangesAsync();
        return Ok(content);
    }
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetMyContents(long userId, [FromServices] FileStorageService storageService)
    {
        var contents = await _db.Contents
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var resultList = new List<object>();

        foreach (var c in contents)
        {
            string? finalUrl = null;
            if (!string.IsNullOrEmpty(c.FileUrl))
            {
                // Dùng chính hàm bạn đã viết trong service
                finalUrl = c.Type == "video"
                    ? await storageService.GetSignedVideoUrl(c.FileUrl)
                    : await storageService.GetSignedPdfUrl(c.FileUrl);
            }

            resultList.Add(new
            {
                c.Id,
                c.Title,
                c.Type,
                c.CreatedAt,
                FullUrl = finalUrl ?? c.YoutubeUrl
            });
        }
        return Ok(resultList);
    }

}
