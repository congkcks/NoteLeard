namespace NoteLearn.Dtos;

public class QaRequest
{
    public long ContentId { get; set; }
    public string Question { get; set; } = "";
    public int TopK { get; set; } = 5;

}
