namespace FastQuiz.Auth.Infrastructure.Services;

public class JwtTokenSettings
{
    // Secrets must come from environment variables or a secret store, never source control.
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");
        if (string.IsNullOrWhiteSpace(Key)
            || System.Text.Encoding.UTF8.GetByteCount(Key) < 32
            || Guid.TryParse(Key, out _)
            || Key.Contains("replace", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Jwt:Key must be an externally supplied random signing key of at least 32 bytes (not a GUID or placeholder).");
    }

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
}
