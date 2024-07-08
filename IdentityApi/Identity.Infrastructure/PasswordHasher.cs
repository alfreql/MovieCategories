using System.Security.Cryptography;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Identity.Infrastructure;

public class PasswordHasher : IPasswordHasher
{
    public (string hashedPassword, string salt) HashPassword(string password)
    {
        // Generate a 128-bit salt using a secure PRNG
        var salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return (hashed, Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string hashedPassword, string salt, string passwordToCheck)
    {
        var saltBytes = Convert.FromBase64String(salt);

        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: passwordToCheck,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed == hashedPassword;
    }
}

