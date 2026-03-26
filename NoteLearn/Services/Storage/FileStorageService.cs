using SupabaseClient = Supabase.Client;
using StorageFileOptions = Supabase.Storage.FileOptions;

namespace NoteLearn.Services.Storage;

public class FileStorageService
{
    private readonly SupabaseClient _supabase;
    private const string DocumentBucket = "documents";
    private const string VideoBucket = "videos";
    private const string BucketName = "documents";
    public FileStorageService(IConfiguration config)
    {
        _supabase = new SupabaseClient(
            config["Supabase:Url"],
            config["Supabase:ServiceKey"]
        );
    }

    public async Task<string> UploadPdfAsync(IFormFile file, long userId, long contentId)
    {
        var filePath = $"user_{userId}/content_{contentId}.pdf";
        return await UploadToSupabase(file, DocumentBucket, filePath, "application/pdf");
    }

    public async Task<string> UploadVideoAsync(IFormFile file, long userId, long contentId)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        var filePath = $"user_{userId}/video_{contentId}{extension}";

        return await UploadToSupabase(file, VideoBucket, filePath, file.ContentType);
    }

    private async Task<string> UploadToSupabase(IFormFile file, string bucket, string filePath, string contentType)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        await _supabase.Storage
            .From(bucket)
            .Upload(
                fileBytes,
                filePath,
                new StorageFileOptions
                {
                    ContentType = contentType,
                    Upsert = true
                }
            );

        return filePath;
    }

    public async Task<string> GetSignedVideoUrl(string filePath)
    {
        return await _supabase.Storage
            .From(VideoBucket)
            .CreateSignedUrl(filePath, 60 * 120);
    }
    public async Task<string> GetSignedPdfUrl(string filePath)
    {
        return await _supabase.Storage
            .From(BucketName)
            .CreateSignedUrl(filePath, 60 * 60);
    }

    public async Task DeleteVideoAsync(string filePath)
    {
        await _supabase.Storage
            .From(VideoBucket)
            .Remove(filePath);
    }
}