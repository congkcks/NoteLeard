
using SupabaseClient = Supabase.Client;
using StorageFileOptions = Supabase.Storage.FileOptions;

namespace NoteLearn.Services.Storage;

public class FileStorageService
{
    private readonly SupabaseClient _supabase;
    private const string BucketName = "documents";

    public FileStorageService(IConfiguration config)
    {
        _supabase = new SupabaseClient(
            config["Supabase:Url"],
            config["Supabase:ServiceKey"]
        );
    }

    // Upload PDF
    public async Task<string> UploadPdfAsync(
        IFormFile file,
        long userId,
        long contentId)
    {
        var filePath = $"user_{userId}/content_{contentId}.pdf";

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        await _supabase.Storage
            .From(BucketName)
            .Upload(
                fileBytes,
                filePath,
                new StorageFileOptions
                {
                    ContentType = "application/pdf",
                    Upsert = true
                }
            );

        return filePath;
    }

    public async Task<string> GetSignedPdfUrl(string filePath)
    {
        return await _supabase.Storage
            .From(BucketName)
            .CreateSignedUrl(filePath, 60 * 60);
    }

    // Xoá file
    public async Task DeletePdfAsync(string filePath)
    {
        await _supabase.Storage
            .From(BucketName)
            .Remove(filePath);
    }
}
