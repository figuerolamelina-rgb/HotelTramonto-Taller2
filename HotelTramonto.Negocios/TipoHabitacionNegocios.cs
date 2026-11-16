using System.Data;
using HotelTramonto.Datos;

namespace HotelTramonto.Negocios
{
    public class TipoHabitacionNegocios
    {
        private readonly ITipoHabitacionDatos tipoHabitacionDatos;

        public TipoHabitacionNegocios() : this(new TipoHabitacionDatos()) { }

        public TipoHabitacionNegocios(ITipoHabitacionDatos tipoHabitacionDatos)
        {
            this.tipoHabitacionDatos = tipoHabitacionDatos
                ?? throw new ArgumentNullException(nameof(tipoHabitacionDatos));
        }

        public DataTable Listar() => tipoHabitacionDatos.Listar();

        public void Insertar(string nombre, string? descripcion, decimal precioNoche,
            int capacidad, string rolUsuario)
        {
            PermisosHabitaciones.ExigirAdministrador(rolUsuario);
            ValidarDatos(nombre, precioNoche, capacidad);
            tipoHabitacionDatos.Insertar(nombre.Trim(), descripcion, precioNoche, capacidad);
        }

        public void Modificar(int idTipoHabitacion, string nombre, string? descripcion,
            decimal precioNoche, int capacidad, string rolUsuario)
        {
            PermisosHabitaciones.ExigirAdministrador(rolUsuario);
            ValidarId(idTipoHabitacion);
            ValidarDatos(nombre, precioNoche, capacidad);
            tipoHabitacionDatos.Modificar(idTipoHabitacion, nombre.Trim(), descripcion, precioNoche, capacidad);
        }

        public void Desactivar(int idTipoHabitacion, string rolUsuario)
        {
            PermisosHabitaciones.ExigirAdministrador(rolUsuario);
            ValidarId(idTipoHabitacion);
            tipoHabitacionDatos.Desactivar(idTipoHabitacion);
        }

        private static void ValidarId(int id)
        {
            if (id <= 0) throw new ArgumentException("Seleccione un tipo de habitación válido.");
        }

        private static void ValidarDatos(string nombre, decimal precioNoche, int capacidad)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
            if (precioNoche <= 0) throw new ArgumentException("El precio debe ser mayor que cero.");
            if (capacidad <= 0) throw new ArgumentException("La capacidad debe ser mayor que cero.");
        }
    }
}
