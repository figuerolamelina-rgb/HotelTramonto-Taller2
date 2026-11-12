
using System;
using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Entidades;

namespace HotelTramonto.Negocios
{
    public class PagoNegocios
    {
        private readonly PagoDatos datos = new PagoDatos();

        public int Insertar(
            int? idReserva,
            int? idReservaTemporal,
            int idUsuario,
            decimal monto,
            string metodoPago,
            string estado,
            string tipoPago)
        {
            if (!idReserva.HasValue && !idReservaTemporal.HasValue)
                throw new Exception(
                    "Debe indicar una reserva o una reserva temporal.");

            if (idReserva.HasValue && idReservaTemporal.HasValue)
                throw new Exception(
                    "El pago no puede asociarse a una reserva y a una reserva temporal al mismo tiempo.");

            if (idUsuario <= 0)
                throw new Exception("El usuario no es válido.");

            if (monto <= 0)
                throw new Exception("El monto debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new Exception("Debe indicar el método de pago.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new Exception("Debe indicar el estado del pago.");

            if (string.IsNullOrWhiteSpace(tipoPago))
                throw new Exception("Debe indicar el tipo de pago.");

            return datos.Insertar(
                idReserva,
                idReservaTemporal,
                idUsuario,
                monto,
                metodoPago.Trim(),
                estado.Trim(),
                tipoPago.Trim());
        }

        public void ActualizarEstado(int idPago, string estado)
        {
            if (idPago <= 0)
                throw new Exception("El ID del pago no es válido.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new Exception("Debe indicar el estado del pago.");

            datos.ActualizarEstado(idPago, estado.Trim());
        }

        public DataTable ListarPorReserva(int idReserva)
        {
            if (idReserva <= 0)
                throw new Exception("El ID de la reserva no es válido.");

            return datos.ListarPorReserva(idReserva);
        }

        public DataTable RegistrarSaldoAlojamiento(
            int idReserva,
            int idUsuario,
            string metodoPago)
        {
            ValidarReservaUsuarioMetodo(idReserva, idUsuario, metodoPago);

            return datos.RegistrarSaldoAlojamiento(
                idReserva,
                idUsuario,
                metodoPago.Trim());
        }

        public DataTable RegistrarServicios(
            int idReserva,
            int idUsuario,
            string metodoPago)
        {
            ValidarReservaUsuarioMetodo(idReserva, idUsuario, metodoPago);

            return datos.RegistrarServicios(
                idReserva,
                idUsuario,
                metodoPago.Trim());
        }

        private void ValidarReservaUsuarioMetodo(
            int idReserva,
            int idUsuario,
            string metodoPago)
        {
            if (idReserva <= 0)
                throw new Exception("El ID de la reserva no es válido.");

            if (idUsuario <= 0)
                throw new Exception("El usuario no es válido.");

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new Exception("Debe indicar el método de pago.");
        }
    }
}
