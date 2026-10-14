namespace FastQuiz.Auth.Infrastructure.Initializer;

/// <summary>Explicit, one-time administrator provisioning. Never resets an existing account.</summary>
public sealed class BootstrapAdminSettings
{
    public bool Enabled { get; set; }
    public string? Password { get; set; }

    public void ValidateForCreation()
    {
        // Login currently accepts at most 20 characters.
        if (string.IsNullOrWhiteSpace(Password) || Password.Length is < 12 or > 20)
            throw new InvalidOperationException("BootstrapAdmin:Password must be supplied externally and contain 12–20 characters.");
    }
}
