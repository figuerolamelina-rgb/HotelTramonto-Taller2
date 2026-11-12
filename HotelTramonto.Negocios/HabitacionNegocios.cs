
using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class HabitacionNegocios
    {
        private readonly HabitacionDatos habitacionDatos = new HabitacionDatos();

        public void Insertar(
            int tipoHabitacion,
            int? piso,
            string numero,
            string estado)
        {
            ValidarDatos(tipoHabitacion, piso, numero, estado);

            habitacionDatos.Insertar(
                tipoHabitacion, piso, numero.Trim(), estado.Trim());
        }

        public void Modificar(
            int idHabitacion,
            int tipoHabitacion,
            int? piso,
            string numero,
            string estado)
        {
            if (idHabitacion <= 0)
                throw new ArgumentException("La habitación seleccionada no es válida.");

            ValidarDatos(tipoHabitacion, piso, numero, estado);

            habitacionDatos.Modificar(
                idHabitacion, tipoHabitacion, piso, numero.Trim(), estado.Trim());
        }

        public void Desactivar(int idHabitacion)
        {
            if (idHabitacion <= 0)
                throw new ArgumentException("La habitación seleccionada no es válida.");

            habitacionDatos.Desactivar(idHabitacion);
        }

        public DataTable Listar()
        {
            return habitacionDatos.Listar();
        }


        public DataTable BuscarPorId(int idHabitacion)
        {
            if (idHabitacion <= 0)
                throw new ArgumentException(
                    "La habitación seleccionada no es válida.");

            DataTable resultado = habitacionDatos.BuscarPorId(idHabitacion);

            if (resultado.Rows.Count == 0)
                throw new ArgumentException(
                    "No se encontró la habitación seleccionada.");

            return resultado;
        }


        public DataTable ConsultarDisponibilidad(
            DateTime fechaEntrada,
            DateTime fechaSalida)
        {
            if (fechaEntrada.Date >= fechaSalida.Date)
                throw new ArgumentException(
                    "La fecha de salida debe ser posterior a la fecha de entrada.");

            return habitacionDatos.ConsultarDisponibilidad(
                fechaEntrada.Date, fechaSalida.Date);
        }

        private void ValidarDatos(
            int tipoHabitacion,
            int? piso,
            string numero,
            string estado)
        {
            if (tipoHabitacion <= 0)
                throw new ArgumentException("Seleccione un tipo de habitación.");

            if (piso.HasValue && piso.Value < 0)
                throw new ArgumentException("El piso no puede ser negativo.");

            if (string.IsNullOrWhiteSpace(numero))
                throw new ArgumentException("Ingrese el número de habitación.");

            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("Seleccione el estado de la habitación.");
        }


        public DataTable ListarInactivas()
        {
            return habitacionDatos.ListarInactivas();
        }

        public void Reactivar(int idHabitacion)
        {
            if (idHabitacion <= 0)
                throw new ArgumentException(
                    "La habitación seleccionada no es válida.");

            habitacionDatos.Reactivar(idHabitacion);
        }

    }
}