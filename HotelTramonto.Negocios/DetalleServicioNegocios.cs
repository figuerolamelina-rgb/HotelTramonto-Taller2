
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class DetalleServicioNegocios
    {
        private readonly IDetalleServicioDatos datos;

        public DetalleServicioNegocios() : this(new DetalleServicioDatos()) { }

        public DetalleServicioNegocios(IDetalleServicioDatos datos)
        {
            this.datos = datos ?? throw new ArgumentNullException(nameof(datos));
        }

        public void Insertar(
            int idServicio,
            int idReserva,
            int cantidad,
            decimal precioUnitario,
            int idHabitacion, Guid idOperacion)
        {
            if (idHabitacion <= 0 || idOperacion == Guid.Empty)
                throw new ArgumentException("Seleccioná una habitación y una operación válidas.");
            if (idServicio <= 0)
                throw new ArgumentException(
                    "El servicio seleccionado no es válido.");

            if (idReserva <= 0)
                throw new ArgumentException(
                    "La reserva seleccionada no es válida.");

            if (cantidad <= 0)
                throw new ArgumentException(
                    "La cantidad debe ser mayor que cero.");

            if (precioUnitario <= 0)
                throw new ArgumentException(
                    "El precio unitario debe ser mayor que cero.");

            if (precioUnitario > 99999999.99m || decimal.Round(precioUnitario, 2) != precioUnitario ||
                (decimal)cantidad * precioUnitario > 99999999.99m)
                throw new ArgumentException("El importe supera el límite o contiene decimales inválidos.");

            datos.Insertar(
                idServicio,
                idReserva,
                cantidad,
                precioUnitario, idHabitacion, idOperacion);
        }

        public DataTable HabitacionesDeReserva(int idReserva)
        {
            if (idReserva <= 0) throw new ArgumentException("La reserva no es válida.");
            return new DetalleReservaDatos().ListarPorReserva(idReserva);
        }

        public DataTable ListarPorReserva(int idReserva, int? idHabitacion = null)
        {
            if (idReserva <= 0 || idHabitacion <= 0)
                throw new ArgumentException(
                    "El ID de la reserva no es válido.");

            return datos.ListarPorReserva(idReserva, idHabitacion);
        }
    }
}
