using System.Data;

namespace HotelTramonto.Negocios;

// El destino de un cobro nunca sobrevive a una nueva búsqueda fallida o sin selección.
public sealed class SeleccionReservaPagos
{
    private readonly HashSet<int> disponibles = [];
    public int IdReserva { get; private set; }

    public void Limpiar() { disponibles.Clear(); IdReserva = 0; }
    public void Deseleccionar() => IdReserva = 0;

    public void Cargar(DataTable reservas)
    {
        Limpiar();
        foreach (DataRow fila in reservas.Rows)
        {
            int id = Convert.ToInt32(fila["id_reserva"]);
            if (id <= 0) throw new ArgumentException("La reserva no es válida.");
            disponibles.Add(id);
        }
    }

    public void Seleccionar(int id)
    {
        IdReserva = 0;
        if (!disponibles.Contains(id))
            throw new ArgumentException("La reserva no pertenece a la consulta actual.");
        IdReserva = id;
    }

    // Solo para la solicitud original recuperada del diario, no para controles nuevos.
    public void Recuperar(int id)
    {
        Limpiar();
        if (id <= 0) throw new ArgumentException("La reserva recuperada no es válida.");
        disponibles.Add(id);
        IdReserva = id;
    }
}
