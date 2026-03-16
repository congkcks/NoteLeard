using Microsoft.AspNetCore.Mvc;
using NoteLearn.Models;

namespace NoteLearn.Controllers;


public class AiController : ControllerBase
{
    private readonly EngLishContext _db;

    public AiController(EngLishContext db)
    {
        _db = db;
    }

    [HttpPost("summary/{contentId}")]
    public async Task<IActionResult> SaveSummary(long contentId, string summary)
    {
        _db.AiMetadata.Add(new AiMetadatum
        {
            ContentId = contentId,
            Summary = summary
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("summary/{contentId}")]
    public IActionResult GetSummary(long contentId)
    {
        return Ok(_db.AiMetadata.FirstOrDefault(x => x.ContentId == contentId));
    }
}

