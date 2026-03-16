using Microsoft.AspNetCore.Mvc;
using NoteLearn.Models;
namespace NoteLearn.Controllers;

public class AuthController: ControllerBase
{
    private readonly EngLishContext _db;
    public AuthController(EngLishContext db)
    {
        _db = db;
    }
    [HttpPost("login")]
    public IActionResult Login(Dtos.LoginRequest login)
    {
        var user = _db.Users.FirstOrDefault(u => u.FullName ==login.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username");
        }
        return Ok(new { Message = "Login successful", UserId = user.Id });

    }
    [HttpPost("register")]
    public async Task<IActionResult> Register(Dtos.LoginRequest register)
    {
        var existingUser = _db.Users.FirstOrDefault(u => u.FullName == register.Username);
        if (existingUser != null)
        {
            return Conflict("Username already exists");
        }
        var newUser = new User
        {
            FullName = register.Username,
            CreatedAt = DateTime.Now
        };
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        return Ok(new { Message = "Registration successful", UserId = newUser.Id });
    }

}
