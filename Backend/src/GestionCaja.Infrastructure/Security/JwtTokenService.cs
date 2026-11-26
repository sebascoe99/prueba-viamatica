using System.Security.Claims;
using System.Text;
using GestionCaja.Application.Common.Interfaces;
using GestionCaja.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GestionCaja.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly IEncryptionService _encryptionService;

    public JwtTokenService(JwtSettings settings, IEncryptionService encryptionService)
    {
        _settings = settings;
        _encryptionService = encryptionService;
    }

    public TokenResult GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        // Claims cifradas: aunque alguien decodifique el token (Base64) no puede leer
        // el id, el usuario ni el rol. Solo la Api, que tiene la clave, los descifra.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtClaimNames.UserId, _encryptionService.Encrypt(user.UserId.ToString())),
            new(JwtClaimNames.UserName, _encryptionService.Encrypt(user.UserName)),
            new(JwtClaimNames.RolId, _encryptionService.Encrypt(user.RolId.ToString())),
            new(JwtClaimNames.RolName, _encryptionService.Encrypt(user.Rol.RolName))
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expiresAt,
            // La firma garantiza que nadie modificó el token
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenResult(token, expiresAt);
    }
}
