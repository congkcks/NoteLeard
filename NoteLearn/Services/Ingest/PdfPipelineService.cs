using NoteLearn.Services.Download;
using NoteLearn.Services.Pdf;
using NoteLearn.Services.Storage;

namespace NoteLearn.Services.Ingest;


public class PdfPipelineService
{
    private readonly FileStorageService _storage;
    private readonly RemoteFileDownloader _downloader;
    private readonly PdfTextExtractor _extractor;
    private readonly ILogger<PdfPipelineService> _logger;

    public PdfPipelineService(
        FileStorageService storage,
        RemoteFileDownloader downloader,
        PdfTextExtractor extractor,
        ILogger<PdfPipelineService> logger)
    {
        _storage = storage;
        _downloader = downloader;
        _extractor = extractor;
        _logger = logger;
    }

    public async Task<string> GetFullTextAsync(string fileUrl, CancellationToken ct)
    {
        string? tempPath = null;
        try
        {
            // Bước 1: Lấy link từ Storage (S3/GCS/Azure)
            var signedUrl = await _storage.GetSignedPdfUrl(fileUrl);
            signedUrl = signedUrl.TrimEnd('?');

            // Bước 2: Download PDF về ổ đĩa tạm
            tempPath = await _downloader.DownloadToTempAsync(signedUrl, ct);

            // Bước 3: Trích xuất toàn bộ text từ file tạm
            var (rawText, _, _) = _extractor.Extract(tempPath);

            return rawText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi Pipeline trích xuất văn bản từ URL: {Url}", fileUrl);
            throw; // Re-throw để Controller xử lý thông báo lỗi
        }
        finally
        {
            // Bước 4: Quan trọng nhất - Dọn dẹp file tạm để không đầy ổ cứng
            if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }
}