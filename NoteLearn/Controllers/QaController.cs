using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoteLearn.Dtos;
using NoteLearn.Models;
using NoteLearn.Services.AI;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace NoteLearn.Controllers;

[ApiController]
[Route("api/qa")]
public class QaController : ControllerBase
{
    private readonly EngLishContext _db;
    private readonly IEmbeddingService _embedder;
    private readonly ILlmService _llm;

    public QaController(
        EngLishContext db,
        IEmbeddingService embedder,
        ILlmService llm)
    {
        _db = db;
        _embedder = embedder;
        _llm = llm;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] QaRequest req, CancellationToken ct)
    {
        if (req == null)
            return BadRequest("Body required.");

        if (string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("Question required.");

        if (req.ContentId <= 0)
            return BadRequest("contentId must be > 0.");

        // tối ưu retrieval
        var topK = req.TopK <= 0 ? 4 : Math.Min(req.TopK, 8);

        // 1️⃣ Embed question
        var qEmbedding = await _embedder.EmbedAsync(req.Question, ct);
        var qVec = new Vector(qEmbedding);

        // 2️⃣ Vector search
        var candidates = await _db.ContentChunks
            .Where(x => x.ContentId == req.ContentId && x.Embedding != null)
            .OrderBy(x => x.Embedding!.L2Distance(qVec))
            .Take(topK * 3) // retrieve nhiều hơn để rerank
            .Select(x => new
            {
                x.Text,
                x.ChunkIndex,
                x.PageNumber,
                Score = x.Embedding!.L2Distance(qVec)
            })
            .ToListAsync(ct);

        if (!candidates.Any())
        {
            return Ok(new
            {
                answer = "Không tìm thấy thông tin trong tài liệu.",
                sources = Array.Empty<object>()
            });
        }

        var chunks = candidates
            .OrderBy(x => x.Score)
            .Take(topK)
            .OrderBy(x => x.PageNumber ?? int.MaxValue)
            .ThenBy(x => x.ChunkIndex)
            .ToList();

        const int maxContextChars = 3000;

        var contextParts = new List<string>();
        int currentLength = 0;

        foreach (var c in chunks)
        {
            if (currentLength + c.Text.Length > maxContextChars)
                break;

            contextParts.Add(c.Text);
            currentLength += c.Text.Length;
        }

        var context = string.Join("\n\n---\n\n", contextParts);

        // 5️⃣ prompt
        var prompt = $@"
Bạn là trợ lý AI cho tài liệu nội bộ.

Chỉ trả lời dựa trên CONTEXT bên dưới.
Chỉ trả lời bằng **tiếng Việt**.
Nếu không có thông tin thì trả lời:
'Không tìm thấy trong tài liệu.'

CONTEXT:
{context}

QUESTION:
{req.Question}

ANSWER (tiếng Việt, ngắn gọn):
";

        // 6️⃣ gọi LLM
        var answer = await _llm.GenerateAsync(prompt, ct);

        return Ok(new
        {
            answer = answer.Trim(),
            sources = chunks.Select((c, i) => new
            {
                source = i + 1,
                c.Text,
                c.ChunkIndex,
                c.PageNumber,
                score = c.Score
            }),
            debug = new
            {
                req.ContentId,
                req.Question,
                topK,
                retrieved = candidates.Count,
                usedChunks = contextParts.Count,
                embeddingDim = qEmbedding.Length
            }
        });
    }
}