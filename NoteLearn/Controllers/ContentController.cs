using Microsoft.AspNetCore.Mvc;
using NoteLearn.Dtos;
using NoteLearn.Models;
using NoteLearn.Services.Background;
using NoteLearn.Services.Storage;
using NoteLearn.Services.Ingest;
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
    public async Task<IActionResult> GetMyContents(long userId, [FromServices] Supabase.Client supabaseClient)
    {
        var contents = _db.Contents
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        var resultList = new List<object>();

        foreach (var c in contents)
        {
            string? finalUrl = c.YoutubeUrl;

            if (c.Type == "pdf" && !string.IsNullOrEmpty(c.FileUrl))
            {
                // Giả sử FileUrl trong DB lưu là: "user_1/content_0.pdf"
                // Ta yêu cầu Supabase tạo một link có thời hạn (ví dụ: 3600 giây = 1 giờ)
                try
                {
                    // "documents" là tên Bucket của bạn trên Supabase
                    finalUrl = await supabaseClient.Storage
                        .From("documents")
                        .CreateSignedUrl(c.FileUrl, 3600);
                }
                catch (Exception)
                {
                    // Nếu lỗi (ví dụ file không tồn tại), trả về link gốc hoặc null
                    finalUrl = c.FileUrl;
                }
            }

            resultList.Add(new
            {
                c.Id,
                c.Title,
                c.Type,
                c.CreatedAt,
                FullUrl = finalUrl // Đây sẽ là link có kèm ?token=...
            });
        }

        return Ok(resultList);
    }

    }
