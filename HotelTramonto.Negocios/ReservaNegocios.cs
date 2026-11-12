
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class ReservaNegocios
    {
        private readonly ReservaDatos reservaDatos = new ReservaDatos();

        public void Cancelar(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("La reserva seleccionada no es válida.");

            reservaDatos.Cancelar(idReserva);
        }

        public DataTable Listar()
        {
            return reservaDatos.Listar();
        }

        public DataTable ListarPorHuesped(int idHuesped)
        {
            if (idHuesped <= 0)
                throw new ArgumentException("Seleccione un huésped válido.");

            return reservaDatos.ListarPorHuesped(idHuesped);
        }

        public DataTable ListarPorFechas(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (fechaDesde.Date > fechaHasta.Date)
                throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");

            return reservaDatos.ListarPorFechas(fechaDesde.Date, fechaHasta.Date);
        }

        public DataTable ListarPorEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("Seleccione un estado.");

            return reservaDatos.ListarPorEstado(estado.Trim());
        }

        public DataTable ConsultarSaldo(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("La reserva seleccionada no es válida.");

            return reservaDatos.ConsultarSaldo(idReserva);
        }

        public DataTable CheckIn(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("La reserva seleccionada no es válida.");

            return reservaDatos.CheckIn(idReserva);
        }

        public DataTable CheckOut(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("La reserva seleccionada no es válida.");

            return reservaDatos.CheckOut(idReserva);
        }

        public DataTable ConfirmarReserva(int idReservaTemporal, int cantidadPersonas)
        {
            if (idReservaTemporal <= 0)
                throw new ArgumentException("La reserva temporal seleccionada no es válida.");

            if (cantidadPersonas <= 0)
                throw new ArgumentException("La cantidad de personas debe ser mayor que cero.");

            return reservaDatos.ConfirmarReserva(idReservaTemporal, cantidadPersonas);
        }
    }
}
