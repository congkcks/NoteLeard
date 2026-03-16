namespace NoteLearn.Dtos;

public record NoteResponse(long Id, int UserId, long ContentId, int? PageNumber, string Text, DateTime CreatedAt);
