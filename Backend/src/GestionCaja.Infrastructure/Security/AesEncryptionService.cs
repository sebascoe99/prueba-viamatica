using System.Security.Cryptography;
using System.Text;
using GestionCaja.Application.Common.Interfaces;

namespace GestionCaja.Infrastructure.Security;

/// <summary>
/// Cifrado simétrico AES-256 (CBC). Cada valor usa un IV aleatorio que se guarda
/// junto al texto cifrado, así el mismo dato nunca produce el mismo resultado.
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    private const int IvSize = 16;
    private readonly byte[] _key;

    public AesEncryptionService(string secret)
    {
        // SHA-256 convierte la clave configurada en los 32 bytes exactos que exige AES-256
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Resultado = IV (16 bytes) + datos cifrados, todo en Base64
        var result = new byte[IvSize + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, IvSize);
        Buffer.BlockCopy(cipherBytes, 0, result, IvSize, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = fullBytes[..IvSize];

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(fullBytes, IvSize, fullBytes.Length - IvSize);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
