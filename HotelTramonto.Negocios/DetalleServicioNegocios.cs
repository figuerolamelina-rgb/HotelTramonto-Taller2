
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class DetalleServicioNegocios
    {
        private readonly DetalleServicioDatos datos =
            new DetalleServicioDatos();

        public void Insertar(
            int idServicio,
            int idReserva,
            int cantidad,
            decimal precioUnitario)
        {
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

            datos.Insertar(
                idServicio,
                idReserva,
                cantidad,
                precioUnitario);
        }

        public DataTable ListarPorReserva(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException(
                    "El ID de la reserva no es válido.");

            return datos.ListarPorReserva(idReserva);
        }
    }
}
