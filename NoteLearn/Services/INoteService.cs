using NoteLearn.Dtos;

namespace NoteLearn.Services;

public interface INoteService
{
    Task<NoteResponse> CreateAsync(int userId, CreateNoteRequest req, CancellationToken ct);
    Task<IReadOnlyList<NoteResponse>> GetByContentAsync(int userId, long contentId, CancellationToken ct);
    Task<NoteResponse> UpdateAsync(int userId, long noteId, UpdateNoteRequest req, CancellationToken ct);
    Task DeleteAsync(int userId, long noteId, CancellationToken ct);
}

