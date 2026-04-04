using System.Security.Cryptography;
using System.Text;
using catalog.Data;
using catalog.Entities;

namespace catalog.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public User? Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == username);

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            return user;
        }

        return null;
    }

    public void Register(User user)
    {
        user.PasswordHash = HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.Now;
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public User? GetByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.Username == username);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}