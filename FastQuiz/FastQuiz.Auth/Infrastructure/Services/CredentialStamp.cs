using System.Security.Cryptography;
using System.Text;

namespace FastQuiz.Auth.Infrastructure.Services;

public static class CredentialStamp
{
    public const string ClaimType = "credential_version";

    public static string Create(string passwordHash, string signingKey) => Convert.ToBase64String(
        HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.UTF8.GetBytes(passwordHash)));

    public static bool Matches(string? claim, string passwordHash, string signingKey)
    {
        if (claim is null) return false;
        try
        {
            return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(claim),
                Convert.FromBase64String(Create(passwordHash, signingKey)));
        }
        catch (FormatException) { return false; }
    }
}
