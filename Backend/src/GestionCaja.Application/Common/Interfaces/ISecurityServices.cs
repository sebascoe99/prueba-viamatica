using GestionCaja.Domain.Entities;

namespace GestionCaja.Application.Common.Interfaces;

// Contratos que la capa Application necesita. Las implementaciones están en Infrastructure.

/// <summary>Cifrado reversible (correo, identificación y claims del JWT).</summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

/// <summary>Hash irreversible para contraseñas.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public record TokenResult(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    TokenResult GenerateToken(User user);
}
