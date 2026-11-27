
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
        public DataTable DeshacerCheckIn(int idReserva)
        {
            if (idReserva <= 0)
                throw new ArgumentException("La reserva seleccionada no es válida.");

            return reservaDatos.DeshacerCheckIn(idReserva);
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

        public DataTable CrearYConfirmar(
    int idHuesped,
    int idHabitacion,
    DateTime fechaEntrada,
    DateTime fechaSalida,
    string modalidadPago,
    int idUsuario,
    decimal monto,
    string metodoPago,
    int cantidadPersonas)
        {
            if (idHuesped <= 0)
                throw new ArgumentException("Seleccione un huésped válido.");

            if (idHabitacion <= 0)
                throw new ArgumentException("Seleccione una habitación válida.");

            if (idUsuario <= 0)
                throw new ArgumentException("El usuario responsable no es válido.");

            if (fechaEntrada.Date < DateTime.Today)
                throw new ArgumentException("La fecha de entrada no puede ser anterior a hoy.");

            if (fechaSalida.Date <= fechaEntrada.Date)
                throw new ArgumentException("La salida debe ser posterior a la entrada.");

            if (cantidadPersonas <= 0)
                throw new ArgumentException("La cantidad de personas debe ser mayor que cero.");

            if (monto <= 0)
                throw new ArgumentException("El pago inicial debe ser mayor que cero.");

            if (modalidadPago != "PAGO_TOTAL" &&
                modalidadPago != "SENA_50")
                throw new ArgumentException("La modalidad de pago no es válida.");

            if (string.IsNullOrWhiteSpace(metodoPago))
                throw new ArgumentException("Seleccione un método de pago.");

            return reservaDatos.CrearYConfirmar(
                idHuesped,
                idHabitacion,
                fechaEntrada.Date,
                fechaSalida.Date,
                modalidadPago,
                idUsuario,
                monto,
                metodoPago.Trim(),
                cantidadPersonas);
        }
    }
}
