using System.Data;

namespace HotelTramonto.Datos;

public interface IComprobantePagoDatos
{
    DataTable ObtenerComprobante(int idPago);
}
