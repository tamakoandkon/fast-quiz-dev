using System.Security.Cryptography;
using System.Text;

namespace FastQuiz.Auth.Infrastructure.Services;

public static class CredentialStamp
{
    public const string ClaimType = "credential_version";

    private static byte[] DeriveSubKey(string signingKey) => HKDF.DeriveKey(
        HashAlgorithmName.SHA256,
        Encoding.UTF8.GetBytes(signingKey),
        outputLength: 32,
        info: "FastQuiz.CredentialStamp"u8.ToArray());

    public static string Create(string passwordHash, string signingKey) => Convert.ToBase64String(
        HMACSHA256.HashData(DeriveSubKey(signingKey), Encoding.UTF8.GetBytes(passwordHash)));

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
