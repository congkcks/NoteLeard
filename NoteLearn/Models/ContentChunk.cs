using Pgvector;

namespace NoteLearn.Models;

public class ContentChunk
{
    public long Id { get; set; }

    public long ContentId { get; set; }         

    public int ChunkIndex { get; set; }

    public int? PageNumber { get; set; }        
    public int? StartTimeSec { get; set; }     
    public int? EndTimeSec { get; set; }

    public string Text { get; set; } = "";

    public Vector Embedding { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public Content Content { get; set; } = default!;
}
