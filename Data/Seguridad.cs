using System.Security.Cryptography;

namespace PlayerasJyC.Data
{
    // Contraseñas protegidas con PBKDF2 (SHA-256, salt aleatorio, 100,000 iteraciones)
    public static class Seguridad
    {
        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string password, string? guardado)
        {
            if (string.IsNullOrEmpty(guardado) || !guardado.Contains('.')) return false;
            try
            {
                var p = guardado.Split('.');
                var salt = Convert.FromBase64String(p[0]);
                var esperado = Convert.FromBase64String(p[1]);
                var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(hash, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
