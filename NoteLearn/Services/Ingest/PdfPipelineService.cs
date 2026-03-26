using NoteLearn.Models;
using NoteLearn.Services.AI;
using NoteLearn.Services.Download;
using NoteLearn.Services.Rag;
using NoteLearn.Services.Storage;

namespace NoteLearn.Services.Pdf;

public class PdfPipelineService
{
    private readonly FileStorageService _storage;
    private readonly RemoteFileDownloader _downloader;
    private readonly PdfTextExtractor _extractor;
    private readonly IEmbeddingService _embeddingService;
    private readonly EngLishContext _db;
    private readonly ILogger<PdfPipelineService> _logger;

    public PdfPipelineService(
        FileStorageService storage,
        RemoteFileDownloader downloader,
        PdfTextExtractor extractor,
        IEmbeddingService embeddingService,
        EngLishContext db,
        ILogger<PdfPipelineService> logger)
    {
        _storage = storage;
        _downloader = downloader;
        _extractor = extractor;
        _embeddingService = embeddingService;
        _db = db;
        _logger = logger;
    }

    public async Task<(int ChunkCount, long ContentId)> IngestPdfToVectorDbAsync(long contentId, CancellationToken ct)
    {
        var content = await _db.Contents.FindAsync(new object[] { contentId }, ct);
        if (content == null) throw new Exception("Content not found");

        string? tempPath = null;
        try
        {
            // 1. Lấy link và dọn dẹp (Xóa dấu ? thừa ở cuối như bạn yêu cầu)
            var signedUrl = await _storage.GetSignedPdfUrl(content.FileUrl);
            signedUrl = signedUrl.TrimEnd('?');

            // 2. Download
            tempPath = await _downloader.DownloadToTempAsync(signedUrl, ct);

            // 3. Extract Full Text
            var (fullText, _, _) = _extractor.Extract(tempPath);
            if (string.IsNullOrWhiteSpace(fullText)) throw new Exception("PDF is empty or non-readable");

            // 4. Chunking (Sử dụng thông số chuẩn: 1000 chars, overlap 200)
            var chunks = TextChunker.Chunk(fullText, 1000, 200).ToList();

            // 5. Xóa các chunk cũ của content này (nếu có) để tránh trùng lặp
            var oldChunks = _db.ContentChunks.Where(c => c.ContentId == contentId);
            _db.ContentChunks.RemoveRange(oldChunks);

            // 6. Loop qua từng chunk để Embed và Save
            int i = 0;
            foreach (var chunkText in chunks)
            {
                var vector = await _embeddingService.EmbedAsync(chunkText, ct);

                var chunkEntity = new ContentChunk
                {
                    ContentId = contentId,
                    ChunkIndex = i,
                    Text = chunkText,
                    Embedding = new Pgvector.Vector(vector),
                    CreatedAt = DateTime.UtcNow
                };

                _db.ContentChunks.Add(chunkEntity);
                i++;
            }

            await _db.SaveChangesAsync(ct);
            return (i, contentId);
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}