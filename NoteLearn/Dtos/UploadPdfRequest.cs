namespace NoteLearn.Dtos;

public class UploadPdfRequest
{
    public IFormFile File { get; set; } = default!;
    public long UserId { get; set; }
}

