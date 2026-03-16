using Microsoft.AspNetCore.Mvc;
using NoteLearn.Models;
using NoteLearn.Services.Storage;
namespace NoteLearn.Controllers;
public class DocumentController : ControllerBase
{
    private readonly EngLishContext _db;
    private readonly FileStorageService _storage;

    public DocumentController(EngLishContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }
    [HttpGet("{contentId}/view")]
    public async Task<IActionResult> ViewPdf(long contentId)
    {
        var content = _db.Contents.Find(contentId);
        if (content == null || content.Type != "pdf")
            return NotFound();

        var signedUrl = await _storage.GetSignedPdfUrl(content.FileUrl);
        return Ok(new { url = signedUrl });
    }
}
