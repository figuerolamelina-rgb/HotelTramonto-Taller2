using System.Data;

namespace HotelTramonto.Datos;

public interface IDetalleServicioDatos
{
    void Insertar(int idServicios, int idReserva, int cantidad, decimal precioUnitario,
        int idHabitacion, Guid idOperacion);
    DataTable ListarPorReserva(int idReserva, int? idHabitacion = null);
}
