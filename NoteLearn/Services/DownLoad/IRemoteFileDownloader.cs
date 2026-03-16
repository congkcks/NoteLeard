namespace NoteLearn.Services.DownLoad;

public interface IRemoteFileDownloader
{
    public Task<string> DownloadToTempAsync(string fileUrl, CancellationToken ct = default);
}
