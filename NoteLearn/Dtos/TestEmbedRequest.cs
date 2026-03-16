namespace NoteLearn.Dtos;

public class TestEmbedRequest
{
    public string Text { get; set; } = "";
    public int ChunkSize { get; set; } = 1500;
    public int Overlap { get; set; } = 200;
    public int MaxChunks { get; set; } = 10;
    public int DelayMs { get; set; } = 400;
}
