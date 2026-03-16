

namespace NoteLearn.Models;

public partial class AiMetadatum
{
    public long Id { get; set; }

    public long? ContentId { get; set; }

    public string? Summary { get; set; }

    public virtual Content? Content { get; set; }
}
