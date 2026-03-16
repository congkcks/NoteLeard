

namespace NoteLearn.Models;

public partial class Collection
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
}
