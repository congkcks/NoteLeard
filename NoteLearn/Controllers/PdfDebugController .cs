using Microsoft.AspNetCore.Mvc;
using NoteLearn.Dtos;
using NoteLearn.Services.Download;
using NoteLearn.Services.Pdf;
namespace NoteLearn.Controllers;
[ApiController]
[Route("api/debug/pdf")]
public class PdfDebugController : ControllerBase
{
    private readonly RemoteFileDownloader _downloader;
    private readonly PdfTextExtractor _extractor;
    private readonly ILogger<PdfDebugController> _logger;

    public PdfDebugController(RemoteFileDownloader downloader, PdfTextExtractor extractor, ILogger<PdfDebugController> logger)
    {
        _downloader = downloader;
        _extractor = extractor;
        _logger = logger;
    }

    [HttpPost("test-link")]
    public async Task<IActionResult> TestLink([FromBody] TestPdfLinkRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Url))
            return BadRequest("Url is required.");

        if (req.PreviewPages <= 0) req.PreviewPages = 3;

        string? tempPdf = null;

        try
        {
            tempPdf = await _downloader.DownloadToTempAsync(req.Url, ct);

            var (_, pages, totalChars) = _extractor.Extract(tempPdf);

            var preview = pages
                .Take(req.PreviewPages)
                .Select(p => new
                {
                    page = p.page,
                    chars = (p.text ?? "").Length,
                    textPreview = (p.text ?? "").Length > 500 ? (p.text[..500] + "...") : p.text
                })
                .ToList();

            return Ok(new
            {
                ok = true,
                pageCount = pages.Count,
                totalChars,
                preview
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test PDF link failed");
            return Ok(new
            {
                ok = false,
                error = ex.Message
            });
        }
        finally
        {
            if (tempPdf != null && System.IO.File.Exists(tempPdf))
            {
                try { System.IO.File.Delete(tempPdf); } catch { }
            }
        }
    }
}
