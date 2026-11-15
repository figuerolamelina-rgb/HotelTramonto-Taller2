using System.Data;

namespace HotelTramonto.Datos;

public interface ITipoHabitacionDatos
{
    void Insertar(string nombre, string? descripcion, decimal precioNoche, int capacidad);
    void Modificar(int idTipoHabitacion, string nombre, string? descripcion, decimal precioNoche, int capacidad);
    void Desactivar(int idTipoHabitacion);
    DataTable Listar();
}
