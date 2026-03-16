using System;
using System.Collections.Generic;

namespace NoteLearn.Models;

public partial class UserProgress
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public long? ContentId { get; set; }

    public int? LastPage { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Content? Content { get; set; }

    public virtual User? User { get; set; }
}
