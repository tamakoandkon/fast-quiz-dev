using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FastQuiz.Auth.Infrastructure.Services;

public class TokenGenerator(IOptions<JwtTokenSettings> settings) : ITokenGenerator
{
    public string GenerateToken(User user)
    {
        var config = settings.Value;
        config.Validate();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
