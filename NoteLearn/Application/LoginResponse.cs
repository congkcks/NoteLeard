namespace NoteLearn.Application;
using MediatR;
public sealed record LoginResponse(long UserId, string Message);
