using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotificaPix.Core.Abstractions.Security;
using NotificaPix.Core.Domain.Entities;
using NotificaPix.Core.Domain.Enums;

namespace NotificaPix.Infrastructure.Security;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string Generate(User user, Organization organization, MembershipRole role)
    {
        var secret = configuration["Security:JwtSecret"] ?? configuration["JWT_SECRET"]
                     ?? throw new InvalidOperationException("JWT secret not configured");

        if (Encoding.UTF8.GetByteCount(secret) < 16)
        {
            throw new InvalidOperationException("JWT secret must be at least 128 bits (16 bytes).");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("orgId", organization.Id.ToString()),
            new(ClaimTypes.Role, role.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
