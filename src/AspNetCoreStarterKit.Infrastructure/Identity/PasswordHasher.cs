// AspNetCoreStarterKit.Infrastructure/Identity/PasswordHasher.cs
using System.Security.Cryptography;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher  // ← Implement interface
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('-');
        if (parts.Length != 2)
            return false;

        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);

        var newHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, hash.Length);
        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }
}