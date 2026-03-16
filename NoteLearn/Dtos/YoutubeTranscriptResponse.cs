namespace NoteLearn.Dtos;
public class YoutubeTranscriptResponse
{
    public string VideoId { get; set; } = string.Empty;
    public object TranscriptRaw { get; set; } = default!;
}