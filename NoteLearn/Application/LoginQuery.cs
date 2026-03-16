namespace NoteLearn.Application;
using MediatR;
public sealed record LoginQuery(string Username,string password): IRequest<LoginResponse>;
