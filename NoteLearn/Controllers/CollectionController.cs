using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteLearn.Models;
namespace NoteLearn.Controllers;
public class CollectionController : ControllerBase
{
    private readonly EngLishContext _db;
    public CollectionController(EngLishContext db)
    {
        _db = db;
    }
    [HttpPost]
    public async Task<IActionResult> Create(string name,int UserId)
    {
        var col = new Collection { Name = name, UserId = UserId };
        _db.Collections.Add(col);
        await _db.SaveChangesAsync();
        return Ok(col);
    }

    [HttpPost("{collectionId}/add/{contentId}")]
    public async Task<IActionResult> AddToCollection(long collectionId, long contentId,int UserId)
    {
        var collection = await _db.Collections
            .Include(c => c.Contents)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == UserId);

        if (collection == null) return NotFound();

        var content = await _db.Contents.FindAsync(contentId);
        if (content == null) return NotFound();

        collection.Contents.Add(content);
        await _db.SaveChangesAsync();

        return Ok();
    }
    [HttpGet]
    public IActionResult GetAll(int UserId)
    {
        return Ok(_db.Collections.Where(x => x.UserId == UserId));
    }
}
