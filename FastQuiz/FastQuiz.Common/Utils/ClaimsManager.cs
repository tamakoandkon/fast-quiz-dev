using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FastQuiz.Common.Utils;

public class ClaimsManager(IHttpContextAccessor httpContextAccessor) : IClaimsManager
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    /// <summary>
    /// 获取用户ID (通常对应 ClaimTypes.NameIdentifier)
    /// </summary>
    public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// 获取用户姓名 (通常对应 ClaimTypes.Name)
    /// </summary>
    public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// 获取所有角色列表
    /// </summary>
    public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

    /// <summary>
    /// 检查用户是否拥有特定角色
    /// </summary>
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;

    public string EnsureAndGetUserId()
    {
        if (string.IsNullOrWhiteSpace(UserId))
        {
            throw new UnauthorizedAccessException();
        }
        return UserId;
    }
}
