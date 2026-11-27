
using System;
using System.Data;
using HotelTramonto.Datos;
using HotelTramonto.Entidades;

namespace HotelTramonto.Negocios
{
    public class PagoNegocios
    {
        private readonly string? rol;
        private readonly IComprobantePagoDatos comprobantes;
        public PagoNegocios(string? rol = null, IComprobantePagoDatos? comprobantes = null)
        {
            this.rol = rol;
            this.comprobantes = comprobantes ?? datos;
        }

        private readonly PagoDatos datos = new PagoDatos();

        public int Insertar(
            int? idReserva,
            int? idReservaTemporal,
            int idUsuario,
            decimal monto,
            string metodoPago,
            string estado,
            string tipoPago, Guid idOperacion)
        {
            PermisosOperacion.Exigir(rol);
            ValidarOperacion(idOperacion);
            if (!idReserva.HasValue && !idReservaTemporal.HasValue)
                throw new Exception(
                    "Debe indicar una reserva o una reserva temporal.");

            if (idReserva.HasValue && idReservaTemporal.HasValue)
                throw new Exception(
                    "El pago no puede asociarse a una reserva y a una reserva temporal al mismo tiempo.");

            if (idReserva <= 0 || idReservaTemporal <= 0)
                throw new ArgumentException("La reserva no es válida.");

            if (idUsuario <= 0)
                throw new Exception("El usuario no es válido.");

            if (monto <= 0 || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
                throw new Exception("El monto debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(metodoPago) || metodoPago.Trim().Length > 30)
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
                tipoPago.Trim(), idOperacion);
        }

        public void ActualizarEstado(int idPago, string estado)
        {
            PermisosOperacion.Exigir(rol);
            if (idPago <= 0)
                throw new Exception("El ID del pago no es válido.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new Exception("Debe indicar el estado del pago.");

            datos.ActualizarEstado(idPago, estado.Trim());
        }



        public DataTable ListarPorReserva(int idReserva)
        {
            PermisosOperacion.Exigir(rol);
            if (idReserva <= 0)
                throw new Exception("El ID de la reserva no es válido.");

            return datos.ListarPorReserva(idReserva);
        }

        public DataTable RegistrarSaldoAlojamiento(
            int idReserva,
            int idUsuario,
            string metodoPago, Guid idOperacion)
        {
            PermisosOperacion.Exigir(rol);
            ValidarOperacion(idOperacion);
            ValidarReservaUsuarioMetodo(idReserva, idUsuario, metodoPago);

            return datos.RegistrarSaldoAlojamiento(
                idReserva,
                idUsuario,
                metodoPago.Trim(), idOperacion);
        }

        public DataTable RegistrarServicios(
            int idReserva,
            int idUsuario,
            string metodoPago, Guid idOperacion)
        {
            PermisosOperacion.Exigir(rol);
            ValidarOperacion(idOperacion);
            ValidarReservaUsuarioMetodo(idReserva, idUsuario, metodoPago);

            return datos.RegistrarServicios(
                idReserva,
                idUsuario,
                metodoPago.Trim(), idOperacion);
        }

        private static void ValidarOperacion(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("La operación es obligatoria.");
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

            if (string.IsNullOrWhiteSpace(metodoPago) || metodoPago.Trim().Length > 30)
                throw new Exception("Debe indicar el método de pago.");
        }
        public DataTable ObtenerComprobante(int idPago)
        {
            PermisosOperacion.Exigir(rol);
            if (idPago <= 0)
                throw new Exception("El ID del pago no es válido.");

            DataTable comprobante = comprobantes.ObtenerComprobante(idPago);

            if (comprobante.Rows.Count == 0)
                throw new Exception(
                    "No se encontró un pago aprobado para generar el comprobante.");

            if (comprobante.Rows.Count != 1)
                throw new InvalidOperationException("El comprobante debe identificar un único pago.");
            ComprobantePagoNegocios.Validar(comprobante.Rows[0], idPago);
            return comprobante;
        }
    }
}
