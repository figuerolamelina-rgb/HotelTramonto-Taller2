using System;
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class ReporteNegocios
    {
        private readonly ReporteDatos datos = new ReporteDatos();

        private void ValidarFechas(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (fechaDesde.Date > fechaHasta.Date)
            {
                throw new ArgumentException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }
        }

        public DataTable ObtenerOcupacion(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            ValidarFechas(fechaDesde, fechaHasta);
            return datos.ObtenerOcupacion(fechaDesde, fechaHasta);
        }

        public DataSet ObtenerIngresos(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            ValidarFechas(fechaDesde, fechaHasta);
            return datos.ObtenerIngresos(fechaDesde, fechaHasta);
        }

        public DataSet ObtenerEstadisticasReservas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            ValidarFechas(fechaDesde, fechaHasta);
            return datos.ObtenerEstadisticasReservas(fechaDesde, fechaHasta);
        }

        public DataTable ObtenerHabitacionesMasUtilizadas(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            ValidarFechas(fechaDesde, fechaHasta);
            return datos.ObtenerHabitacionesMasUtilizadas(fechaDesde, fechaHasta);
        }

        public DataTable ObtenerServiciosMasUtilizados(
            DateTime fechaDesde,
            DateTime fechaHasta)
        {
            ValidarFechas(fechaDesde, fechaHasta);
            return datos.ObtenerServiciosMasUtilizados(fechaDesde, fechaHasta);
        }
    }
}