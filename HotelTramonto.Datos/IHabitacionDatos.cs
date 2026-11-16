using System.Data;

namespace HotelTramonto.Datos;

public interface IHabitacionDatos
{
    void Insertar(int tipoHabitacion, int? piso, string numero, string estado);
    void Modificar(int idHabitacion, int tipoHabitacion, int? piso, string numero, string estado);
    void Desactivar(int idHabitacion);
    void Reactivar(int idHabitacion);
    DataTable Listar();
    DataTable BuscarPorId(int idHabitacion);
    DataTable ListarInactivas();
    DataTable ConsultarDisponibilidad(DateTime fechaEntrada, DateTime fechaSalida);
}
