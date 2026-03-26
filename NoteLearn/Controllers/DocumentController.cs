using Microsoft.AspNetCore.Mvc;
using NoteLearn.Services.Pdf;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly PdfPipelineService _pipeline;

    public DocumentController(PdfPipelineService pipeline)
    {
        _pipeline = pipeline;
    }

    [HttpPost("{contentId}/ingest-full")]
    public async Task<IActionResult> IngestFull(long contentId, CancellationToken ct)
    {
        try
        {
            // Một lệnh duy nhất: Download -> Extract -> Chunk -> Embed -> Save DB
            var (chunkCount, id) = await _pipeline.IngestPdfToVectorDbAsync(contentId, ct);

            return Ok(new
            {
                ok = true,
                message = "Tài liệu đã được trích xuất và lưu vào Vector DB thành công",
                contentId = id,
                totalChunks = chunkCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }

}