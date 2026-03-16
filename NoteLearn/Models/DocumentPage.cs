using System;
using System.Collections.Generic;

namespace NoteLearn.Models;

public partial class DocumentPage
{
    public long Id { get; set; }

    public long? ContentId { get; set; }

    public int PageNumber { get; set; }

    public string? Text { get; set; }

    public virtual Content? Content { get; set; }
}
