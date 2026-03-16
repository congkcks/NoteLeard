using Microsoft.AspNetCore.Mvc;
using NoteLearn.Models;
namespace NoteLearn.Controllers;
public class NoteController : ControllerBase
{
    private readonly EngLishContext _db;
    public NoteController(EngLishContext db)
    {
        _db = db;
    }
    [HttpPost]
    public async Task<IActionResult> Create(long contentId, int? page, string text,int UserId)
    {
        var note = new Note
        {
            UserId = UserId,
            ContentId = contentId,
            PageNumber = page,
            Text = text,
            CreatedAt = DateTime.Now
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return Ok(note);
    }
    [HttpGet("{contentId}")]
    public IActionResult GetNotes(long contentId, int UserId)
    {
        return Ok(_db.Notes
            .Where(n => n.ContentId == contentId && n.UserId == UserId)
            .OrderByDescending(n => n.CreatedAt));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, string text)
    {
        var note = _db.Notes.Find(id);
        if (note == null) return NotFound();

        note.Text = text;
        await _db.SaveChangesAsync();
        return Ok(note);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var note = _db.Notes.Find(id);
        if (note == null) return NotFound();

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
