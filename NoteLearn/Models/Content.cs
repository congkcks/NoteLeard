
namespace NoteLearn.Models;

public partial class Content
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public string? FileUrl { get; set; }

    public string? YoutubeUrl { get; set; }

    public int? TotalPages { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AiMetadatum> AiMetadata { get; set; } = new List<AiMetadatum>();

    public virtual ICollection<DocumentPage> DocumentPages { get; set; } = new List<DocumentPage>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserProgress> UserProgresses { get; set; } = new List<UserProgress>();

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();
}
