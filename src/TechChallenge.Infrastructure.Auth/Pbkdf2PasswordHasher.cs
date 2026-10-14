using System.Security.Cryptography;
using TechChallenge.Application.Abstractions.Auth;

namespace TechChallenge.Infrastructure.Auth
{
    // PBKDF2 (SHA-256) usando apenas System.Security.Cryptography — sem dependências externas.
    // Formato armazenado: {iteracoes}.{saltBase64}.{hashBase64}
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string senha)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, Algorithm, KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string senha, string hash)
        {
            var partes = hash.Split('.', 3);
            if (partes.Length != 3)
                return false;

            if (!int.TryParse(partes[0], out var iteracoes))
                return false;

            byte[] salt;
            byte[] esperado;
            try
            {
                salt = Convert.FromBase64String(partes[1]);
                esperado = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, Algorithm, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
