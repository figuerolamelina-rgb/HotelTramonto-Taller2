
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
            return !string.IsNullOrWhiteSpace(dni)
                && dni.Trim().Length >= 7
                && dni.Trim().Length <= 15;
        }

        public static bool EmailValido(string? email)
        {
            return !string.IsNullOrWhiteSpace(email)
                && email.Contains("@")
                && email.Contains(".");
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
    }
}
