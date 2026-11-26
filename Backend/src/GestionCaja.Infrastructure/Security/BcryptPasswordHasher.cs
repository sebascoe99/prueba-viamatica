using GestionCaja.Application.Common.Interfaces;

namespace GestionCaja.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con BCrypt. Es irreversible: para validar se vuelve a calcular
/// el hash con la misma sal (que va incluida en el propio hash) y se compara.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
