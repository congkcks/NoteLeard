using Microsoft.AspNetCore.Mvc;
using NoteLearn.Dtos;
using NoteLearn.Models;
using NoteLearn.Services.AI;
using NoteLearn.Services.Rag;
using Pgvector;
namespace NoteLearn.Controllers;
[ApiController]
[Route("api/debug/embedding")]
public class EmbeddingDebugController :ControllerBase
{
    private readonly EngLishContext _db;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<EmbeddingDebugController> _logger;
    public EmbeddingDebugController(
        EngLishContext db,
        IEmbeddingService embeddingService,
        ILogger<EmbeddingDebugController> logger)
    {
        _db = db;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    [HttpPost("chunk-and-embed")]
    public async Task<IActionResult> ChunkAndEmbed([FromBody] TestEmbedRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req?.Text))
            return BadRequest("Text is required.");

        var chunkSize = req.ChunkSize > 0 ? req.ChunkSize : 1500;
        var overlap = req.Overlap >= 0 ? req.Overlap : 0;
        var maxChunks = req.MaxChunks > 0 ? req.MaxChunks : 10;
        var delayMs = req.DelayMs > 0 ? req.DelayMs : 0;

        var chunks = TextChunker.Chunk(req.Text, chunkSize, overlap).Take(maxChunks);

        var processed = new List<object>();
        var i = 0;

        foreach (var ch in chunks)
        {
            try
            {
                if (delayMs > 0 && i > 0) await Task.Delay(delayMs, ct);

                var vec = await _embeddingService.EmbedAsync(ch, ct);

                processed.Add(new
                {
                    index = i,
                    chars = ch.Length,
                    vectorDim = vec.Length,
                    preview = ch.Length > 200 ? ch[..200] + "..." : ch
                });

                i++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Embedding failed at chunk {ChunkIndex}", i);
                return Ok(new { ok = false, failedAtChunk = i, error = ex.Message, processed });
            }
        }

        return Ok(new { ok = true, totalChars = req.Text.Length, chunkCount = i, processed });
    }
    [HttpPost("ingest/6")]
    public async Task<IActionResult> IngestForContent6(
    [FromBody] IngestTestRequest req,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest();

        const int chunkSize = 200;
        const int overlap = 50;

        var chunks = TextChunker.Chunk(req.Text, chunkSize, overlap).ToList();

        var content = await _db.Contents.FindAsync(6L);
        if (content == null) return NotFound("Content 6 not found");

        var saved = new List<object>();
        var i = 0;

        foreach (var ch in chunks)
        {
            var vec = await _embeddingService.EmbedAsync(ch, ct);

            var row = new ContentChunk
            {
                ContentId = 6,
                ChunkIndex = i,
                Text = ch,
                Embedding = new Vector(vec),
                CreatedAt = DateTime.UtcNow
            };

            _db.ContentChunks.Add(row);
            await _db.SaveChangesAsync(ct);

            saved.Add(new { i, ch = ch[..Math.Min(80, ch.Length)] });
            i++;
        }

        return Ok(new
        {
            ok = true,
            chunks = saved.Count,
            saved
        });
    }
}
