using MediatR;
using NoteLearn.Models;

namespace NoteLearn.Application;

public sealed class LoginQueryHandler
    : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly EngLishContext _db;

    public LoginQueryHandler(EngLishContext db)
    {
        _db = db;
    }

    public Task<LoginResponse> Handle(
        LoginQuery request,
        CancellationToken cancellationToken)
    {
        var user = _db.Users
            .FirstOrDefault(u => u.FullName == request.Username);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username");
        }

        return Task.FromResult(
            new LoginResponse(user.Id, "Login successful"));
    }
}
