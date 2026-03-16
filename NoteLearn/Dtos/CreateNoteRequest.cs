namespace NoteLearn.Dtos;

public sealed  record CreateNoteRequest(long ContentId, int? page, string Text);

