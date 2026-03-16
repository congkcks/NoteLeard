using NoteLearn.Models;

namespace NoteLearn.Services;

public interface INoteRepository
{
    Task AddAsync(Note note, CancellationToken ct);
    Task<Note?> GetByIdAsync(long id, CancellationToken ct);
    Task<IReadOnlyList<Note>> GetByContentAsync(int userId, long contentId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    void Remove(Note note);
}

