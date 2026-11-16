using System.Globalization;

namespace HotelTramonto.Presentacion;

internal static class PresentacionTablas
{
    private static readonly Dictionary<string, string> Titulos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id_tipoHabitacion"] = "ID de tipo de habitación", ["tipo_habitacion"] = "Tipo de habitación",
        ["id_usuario"] = "ID de empleado", ["fecha_alta"] = "Fecha de alta",
        ["descripcion"] = "Descripción", ["numero"] = "Número", ["dni"] = "DNI",
        ["email"] = "Correo electrónico", ["telefono"] = "Teléfono", ["categoria"] = "Categoría",
        ["precio"] = "Precio", ["total"] = "Total", ["subtotal"] = "Subtotal",
        ["cantidad"] = "Cantidad", ["activo"] = "Activo",
        ["id_reserva"] = "N.º de reserva", ["id_habitacion"] = "ID de habitación",
        ["id_huesped"] = "ID de huésped", ["id_pago"] = "N.º de pago",
        ["numero_habitacion_actual"] = "Número de habitación", ["numero_habitacion"] = "Habitación",
        ["tipo_habitacion_actual"] = "Tipo de habitación", ["fecha_creacion"] = "Fecha de reserva",
        ["fecha_entrada"] = "Entrada prevista", ["fecha_salida"] = "Salida prevista",
        ["fecha_checkin"] = "Check-in", ["fecha_checkout"] = "Check-out",
        ["monto"] = "Importe", ["precio_noche"] = "Precio por noche",
        ["precio_unitario"] = "Precio unitario", ["tipo_pago"] = "Concepto del pago",
        ["metodo_pago"] = "Método de pago", ["estado"] = "Estado",
        ["nombre_servicio"] = "Servicio", ["nombre_huesped"] = "Nombre del huésped",
        ["apellido_huesped"] = "Apellido del huésped", ["id_operacion"] = "Operación",
        ["fecha_pago"] = "Fecha de pago", ["modalidad_pago"] = "Modalidad de pago"
    };

    public static void Conectar(Control control)
    {
        if (control is DataGridView tabla)
        {
            tabla.DataBindingComplete += (_, _) => Aplicar(tabla);
            Aplicar(tabla);
        }
        control.ControlAdded += (_, e) => { if (e.Control != null) Conectar(e.Control); };
        foreach (Control hijo in control.Controls) Conectar(hijo);
    }

    private static void Aplicar(DataGridView tabla)
    {
        tabla.ScrollBars = ScrollBars.Both;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        foreach (DataGridViewColumn columna in tabla.Columns)
        {
            string nombre = columna.DataPropertyName.Length > 0 ? columna.DataPropertyName : columna.Name;
            columna.HeaderText = Titulos.TryGetValue(nombre, out var titulo) ? titulo :
                CultureInfo.GetCultureInfo("es-AR").TextInfo.ToTitleCase(nombre.Replace('_', ' '));
            columna.MinimumWidth = 85;
            columna.Width = nombre.Contains("nombre") || nombre.Contains("apellido") ? 165 : 130;
            columna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            if (columna.ValueType == typeof(DateTime))
            {
                columna.DefaultCellStyle.Format = nombre is "fecha_entrada" or "fecha_salida" ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm";
                columna.Width = 155;
            }
            if (columna.ValueType == typeof(decimal))
            {
                columna.DefaultCellStyle.Format = "N2";
                columna.DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("es-AR");
                columna.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
}
