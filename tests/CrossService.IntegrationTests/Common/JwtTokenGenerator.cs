using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CrossService.IntegrationTests.Common;

public static class JwtTokenGenerator
{
    private const string SecretKey = "ChaveSecretaMuitoSeguraParaJWTMecanicaOS2024!@#$%";
    private const string Issuer = "ms-cadastros";
    private const string Audience = "mecanicaos";

    public static string GenerateToken(
        string userId = "test-user-id",
        string email = "test@test.com",
        string role = "Admin",
        int expirationMinutes = 120)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GerarToken(
        string email,
        string nome,
        string role,
        string? userId = null)
    {
        return GenerateToken(
            userId: userId ?? Guid.NewGuid().ToString(),
            email: email,
            role: role);
    }

    public static string GerarTokenExpirado()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "expired-user"),
            new Claim(JwtRegisteredClaimNames.Email, "expired@test.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-30), // Token expirado
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
