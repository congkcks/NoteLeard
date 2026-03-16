namespace NoteLearn.Services.Download;

public class RemoteFileDownloader
{
    private readonly HttpClient _http;

    public RemoteFileDownloader(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<string> DownloadToTempAsync(string fileUrl, CancellationToken ct = default)
    {
        Directory.CreateDirectory("tmp");
        var tempPath = Path.Combine("tmp", $"{Guid.NewGuid():N}.pdf");
        
        await using var stream = await _http.GetStreamAsync(fileUrl, ct);
        await using var fs = File.Create(tempPath);
        await stream.CopyToAsync(fs, ct);

        return tempPath;
    }
}
