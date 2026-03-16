using System;
using System.Collections.Generic;

namespace NoteLearn.Models;

public partial class Note
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? ContentId { get; set; }

    public int? PageNumber { get; set; }

    public string Text { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Content? Content { get; set; }

    public virtual User? User { get; set; }
}
