namespace NoteLearn.Dtos;

public class TestPdfChunkRequest
{
    public long ContentId { get; set; }
    public string Url { get; set; } = "";
    public int MaxPages { get; set; } = 5;      // để test nhanh
    public int MaxChunks { get; set; } = 20;    // để khỏi tốn tiền embedding
}
