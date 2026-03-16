using Microsoft.EntityFrameworkCore;
using NoteLearn.Models;
using NoteLearn.Services.AI;
using NoteLearn.Services.Download;
using NoteLearn.Services.Pdf;
using NoteLearn.Services.Rag;
using NoteLearn.Services.Storage;
using Pgvector;
namespace NoteLearn.Services.Ingest;
public class PdfRagIngestJob
{
    private readonly EngLishContext _db;
    private readonly RemoteFileDownloader _downloader;
    private readonly PdfTextExtractor _extractor;
    private readonly IEmbeddingService _embedder;
    private readonly ILogger<PdfRagIngestJob> _logger;
    private readonly FileStorageService _storage;
    public PdfRagIngestJob(
        EngLishContext db,
        RemoteFileDownloader downloader,
        PdfTextExtractor extractor,
        IEmbeddingService embedder,
        ILogger<PdfRagIngestJob> logger,
        FileStorageService storage)
    {
        _db = db;
        _downloader = downloader;
        _extractor = extractor;
        _embedder = embedder;
        _logger = logger;
        _storage = storage;
    }

    public async Task RunAsync(long contentId, CancellationToken ct = default)
    {
        var doc = await _db.Contents.FirstOrDefaultAsync(x => x.Id == contentId, ct);
        var signedUrl = await _storage.GetSignedPdfUrl(doc.FileUrl);
        Console.WriteLine("Signed URL: " + signedUrl);
        if (doc == null) return;

        if (string.IsNullOrWhiteSpace(doc.FileUrl))
            throw new InvalidOperationException("Content.FileUrl is null/empty.");

        string? tempPdf = null;

        try
        {
            _db.ContentChunks.RemoveRange(_db.ContentChunks.Where(x => x.ContentId == contentId));
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Old content chunks deleted. contentId={ContentId}", contentId);
            tempPdf = await _downloader.DownloadToTempAsync(signedUrl, ct);

            var (_, pages, totalChars) = _extractor.Extract(tempPdf);

            doc.TotalPages = pages.Count;
            await _db.SaveChangesAsync(ct);

            if (totalChars < 200)
            {
                _logger.LogWarning("PDF contentId={ContentId} looks image-based (needs OCR).", contentId);
                return;
            }

            int chunkIndex = 0;

            foreach (var p in pages)
            {
                if (string.IsNullOrWhiteSpace(p.text)) continue;

                var chunks = TextChunker.Chunk(p.text, maxChunkLength: 1800, overlap: 200);

                foreach (var ch in chunks)
                {
                    var vec = await _embedder.EmbedAsync(ch, ct);

                    _db.ContentChunks.Add(new ContentChunk
                    {
                        ContentId = contentId,   
                        ChunkIndex = chunkIndex++,
                        PageNumber = p.page,     
                        Text = ch,
                        Embedding = new Vector(vec),
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "PDF ingest done. contentId={ContentId}, pages={Pages}, chunks={Chunks}",
                contentId, pages.Count, chunkIndex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF ingest failed. contentId={ContentId}", contentId);
            throw;
        }
        finally
        {
            if (tempPdf != null && File.Exists(tempPdf))
            {
                try { File.Delete(tempPdf); } catch { }
            }
        }
    }
}

