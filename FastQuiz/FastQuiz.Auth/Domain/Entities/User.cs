using FastQuiz.Auth.Domain.Services;

namespace FastQuiz.Auth.Domain.Entities;

public class User
{
    public const string DefaultAdminId = "default_admin";
    public const string DefaultAdminUsername = "defaultadmin";
    public const string DefaultAdminPassword = "fastquiz";

    public string Id { get; private set; } = null!;
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    private User() { }

    public User(string id, string username, string? email, string passwordHash)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }

    public void SetPassword(string password, IPasswordHasher passwordHasher)
    {
        PasswordHash = passwordHasher.Hash(password);
    }

    public void SetEmail(string? email)
    {
        Email = email;
    }

    public void SetUsername(string username)
    {
        Username = username;
    }
}
