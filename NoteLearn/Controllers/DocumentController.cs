using Microsoft.AspNetCore.Mvc;
using NoteLearn.Models;
using NoteLearn.Services.Ingest;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly EngLishContext _db;
    private readonly PdfPipelineService _pipeline;

    public DocumentController(EngLishContext db, PdfPipelineService pipeline)
    {
        _db = db;
        _pipeline = pipeline;
    }

    [HttpGet("{contentId}/extract-test")]
    public async Task<IActionResult> ExtractTest(long contentId, CancellationToken ct)
    {
        // 1. Kiểm tra tài liệu trong DB
        var content = await _db.Contents.FindAsync(contentId);
        if (content == null)
            return NotFound(new { ok = false, message = "Không tìm thấy Content ID" });

        if (content.Type != "pdf")
            return BadRequest(new { ok = false, message = "Tài liệu này không phải định dạng PDF" });

        try
        {
            // 2. Chạy Pipeline lấy Full Text
            string fullText = await _pipeline.GetFullTextAsync(content.FileUrl, ct);

            // 3. Trả về kết quả thô để kiểm tra
            return Ok(new
            {
                ok = true,
                contentId = content.Id,
                title = content.Title,
                charCount = fullText.Length,
                fullText = fullText // Kết quả test quan trọng nhất ở đây
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }
}