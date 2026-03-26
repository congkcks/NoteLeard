namespace NoteLearn.Dtos
{
    public class UploadVideoRequest
    {
        public IFormFile File { get; set; }
        public long UserId { get; set; }
    }
}
