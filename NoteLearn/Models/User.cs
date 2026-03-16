

namespace NoteLearn.Models;

public partial class User
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();
}
