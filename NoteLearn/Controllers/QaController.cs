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
        // 1. Kiểm tra đầu vào cơ bản
        if (req == null || string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("Question is required.");

        var topK = req.TopK <= 0 ? 4 : Math.Min(req.TopK, 8);

        // 2. Chuyển câu hỏi sang Vector
        var qEmbedding = await _embedder.EmbedAsync(req.Question, ct);
        var qVec = new Vector(qEmbedding);

        // 3. LẤY CONTEXT ƯU TIÊN (Trang bìa/Metadata)
        // Luôn lấy ChunkIndex = 0 vì đây thường là nơi chứa tiêu đề, tác giả, ngày tháng.
        var metadataChunk = await _db.ContentChunks
            .Where(x => x.ContentId == req.ContentId && x.ChunkIndex == 0)
            .Select(x => new { x.Text, x.ChunkIndex, x.PageNumber, Score = 0.0f })
            .FirstOrDefaultAsync(ct);

        // 4. TÌM KIẾM VECTOR (Các đoạn liên quan nhất)
        var vectorCandidates = await _db.ContentChunks
            .Where(x => x.ContentId == req.ContentId && x.ChunkIndex != 0 && x.Embedding != null)
            .OrderBy(x => x.Embedding!.L2Distance(qVec))
            .Take(topK)
            .Select(x => new
            {
                x.Text,
                x.ChunkIndex,
                x.PageNumber,
                Score = (float)x.Embedding!.L2Distance(qVec)
            })
            .ToListAsync(ct);

        // 5. HỢP NHẤT DỮ LIỆU
        var finalChunks = new List<dynamic>();
        if (metadataChunk != null) finalChunks.Add(metadataChunk);

        // Chỉ thêm các chunk vector nếu chúng chưa tồn tại (tránh trùng lặp)
        foreach (var candidate in vectorCandidates)
        {
            if (!finalChunks.Any(c => c.ChunkIndex == candidate.ChunkIndex))
                finalChunks.Add(candidate);
        }

        if (!finalChunks.Any())
            return Ok(new { answer = "Không tìm thấy dữ liệu.", sources = Array.Empty<object>() });

        // Sắp xếp lại theo thứ tự logic của tài liệu (Page -> Index) để LLM dễ đọc
        var sortedChunks = finalChunks
            .OrderBy(x => x.PageNumber ?? int.MaxValue)
            .ThenBy(x => x.ChunkIndex)
            .ToList();

        // 6. GHÉP CONTEXT VÀ NHẮC NHỞ LLM
        var contextParts = sortedChunks.Select(c => $"[Đoạn {c.ChunkIndex}]: {c.Text}").ToList();
        var context = string.Join("\n\n", contextParts);

        var prompt = $@"
Bạn là trợ lý AI cho tài liệu học tập NoteLearn.
Thông tin về tiêu đề, tác giả, ngày tháng thường nằm ở các đoạn đầu tiên (Đoạn 0).

NHIỆM VỤ:
1. Chỉ trả lời dựa trên CONTEXT bên dưới.
2. Trả lời bằng tiếng Việt, ngắn gọn, chính xác.
3. Nếu CONTEXT không có thông tin, hãy nói 'Không tìm thấy trong tài liệu.'

CONTEXT:
{context}

QUESTION: {req.Question}

ANSWER:";

        // 7. GỌI LLM VÀ TRẢ KẾT QUẢ
        var answer = await _llm.GenerateAsync(prompt, ct);

        return Ok(new
        {
            answer = answer.Trim(),
            sources = sortedChunks,
            debug = new
            {
                req.ContentId,
                usedMetadataChunk = metadataChunk != null,
                totalChunksUsed = finalChunks.Count
            }
        });
    }
}