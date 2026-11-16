
using System;
using System.Linq;
using System.Net.Mail;

namespace HotelTramonto.Negocios
{
    public class Validaciones
    {
        public static bool TextoObligatorio(string? texto)
        {
            return !string.IsNullOrWhiteSpace(texto);
        }

        public static bool DNIValido(string? dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return false;

            dni = dni.Trim();

            return dni.Length >= 7
                && dni.Length <= 15
                && dni.All(char.IsDigit);
        }

        public static bool EmailValido(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var direccion = new MailAddress(email.Trim());

                return direccion.Address == email.Trim()
                    && direccion.Host.Contains(".")
                    && !direccion.Host.StartsWith(".")
                    && !direccion.Host.EndsWith(".");
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool FechasReservaValidas(
            DateTime fechaEntrada,
            DateTime fechaSalida)
        {
            return fechaSalida.Date > fechaEntrada.Date;
        }

        public static bool MontoValido(decimal monto)
        {
            return monto > 0;
        }

        public static bool HuespedValido(
            string? nombre,
            string? apellido,
            string? dni,
            string? email,
            string? nacionalidad)
        {
            return TextoObligatorio(nombre)
                && TextoObligatorio(apellido)
                && DNIValido(dni)
                && (string.IsNullOrWhiteSpace(email)
                    || EmailValido(email))
                && TextoObligatorio(nacionalidad);
        }
    }
}
