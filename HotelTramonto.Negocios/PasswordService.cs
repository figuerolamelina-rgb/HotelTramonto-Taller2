
using Microsoft.AspNetCore.Identity;

namespace HotelTramonto.Negocios
{
    public static class PasswordService
    {
        private static readonly PasswordHasher<string> hasher = new();

        public static string Hash(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentException(
                    "La contraseña es obligatoria.",
                    nameof(contrasena));
            }

            return hasher.HashPassword(
                "HotelTramonto",
                contrasena);
        }

        public static bool Verificar(
            string hash,
            string contrasena)
        {
            if (string.IsNullOrWhiteSpace(hash) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                return false;
            }

            var resultado = hasher.VerifyHashedPassword(
                "HotelTramonto",
                hash,
                contrasena);

            return resultado != PasswordVerificationResult.Failed;
        }
    }
}
