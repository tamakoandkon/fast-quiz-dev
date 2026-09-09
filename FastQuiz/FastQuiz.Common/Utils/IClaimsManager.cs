namespace FastQuiz.Common.Utils;

public interface IClaimsManager
{
    string? UserId { get; }
    string? UserName { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
    string EnsureAndGetUserId();
}
