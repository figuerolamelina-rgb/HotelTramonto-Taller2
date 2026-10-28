
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class ReservaTemporalNegocios
    {
        private readonly ReservaTemporalDatos datos =
            new ReservaTemporalDatos();

        public int Insertar(
            int idHuesped,
            int idHabitacion,
            DateTime fechaEntrada,
            DateTime fechaSalida,
            string modalidadPago)
        {
            if (idHuesped <= 0)
                throw new ArgumentException(
                    "Debe seleccionar un huésped válido.");

            if (idHabitacion <= 0)
                throw new ArgumentException(
                    "Debe seleccionar una habitación válida.");

            if (fechaEntrada.Date < DateTime.Today)
                throw new ArgumentException(
                    "La fecha de entrada no puede ser anterior a hoy.");

            if (fechaSalida.Date <= fechaEntrada.Date)
                throw new ArgumentException(
                    "La fecha de salida debe ser posterior a la fecha de entrada.");

            if (string.IsNullOrWhiteSpace(modalidadPago))
                throw new ArgumentException(
                    "Debe seleccionar una modalidad de pago.");

            modalidadPago = modalidadPago.Trim().ToUpperInvariant();

            if (modalidadPago != "PAGO_TOTAL" &&
                modalidadPago != "SENA_50")
                throw new ArgumentException(
                    "La modalidad debe ser PAGO_TOTAL o SENA_50.");

            return datos.Insertar(
                idHuesped,
                idHabitacion,
                fechaEntrada.Date,
                fechaSalida.Date,
                modalidadPago);
        }

        public void ActualizarEstado(
            int idReservaTemporal,
            string estado)
        {
            if (idReservaTemporal <= 0)
                throw new ArgumentException(
                    "La reserva temporal seleccionada no es válida.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException(
                    "Debe indicar el estado de la reserva temporal.");

            datos.ActualizarEstado(
                idReservaTemporal,
                estado.Trim().ToUpperInvariant());
        }

        public void ExpirarVencidas()
        {
            datos.ExpirarVencidas();
        }

        public DataTable ListarPendientes()
        {
            return datos.ListarPendientes();
        }
    }
}
