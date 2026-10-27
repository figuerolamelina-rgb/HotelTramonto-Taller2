
using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class TipoHabitacionNegocios
    {
        private readonly TipoHabitacionDatos tipoHabitacionDatos =
            new TipoHabitacionDatos();

        public DataTable Listar()
        {
            return tipoHabitacionDatos.Listar();
        }
    }
}
