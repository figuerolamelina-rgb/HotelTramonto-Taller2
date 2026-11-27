using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HotelTramonto.Datos;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.Negocios;

/// <summary>Diario local por identidad Windows y destino SQL. No guarda credenciales.</summary>
public sealed class DiarioOperaciones
{
    private readonly string directorio;
    public sealed record Registro(Guid Id, string Solicitud, int? Referencia, bool CreacionIniciada = false);
    public sealed record Resultado<T>(Guid Id, T Valor);

    public sealed class Contexto
    {
        private readonly Action<int> guardar;
        private readonly Action marcar;
        public bool CreacionIniciada { get; private set; }
        public Guid Id { get; }
        public int? Referencia { get; private set; }
        internal Contexto(Registro registro, Action<int> guardar, Action marcar)
        {
            Id = registro.Id;
            Referencia = registro.Referencia;
            this.guardar = guardar;
            this.marcar = marcar;
            CreacionIniciada = registro.CreacionIniciada;
        }
        public void MarcarCreacion()
        {
            if (CreacionIniciada || Referencia.HasValue)
                throw new InvalidOperationException("La creación ya fue iniciada; requiere conciliación.");
            marcar();
            CreacionIniciada = true;
        }

        public void GuardarReferencia(int valor)
        {
            if (valor <= 0 || (Referencia.HasValue && Referencia != valor))
                throw new InvalidOperationException("Referencia de operación inválida.");
            guardar(valor);
            Referencia = valor;
            CreacionIniciada = false;
        }
    }

    public DiarioOperaciones(string directorio)
    {
        this.directorio = directorio;
        Directory.CreateDirectory(directorio);
    }

    public static DiarioOperaciones Actual()
    {
        using var conexion = ConexionBD.ObtenerConexion(); // Solo lee configuración, no abre SQL.
        string destino = conexion.DataSource + "\n" + conexion.Database;
        return new DiarioOperaciones(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HotelTramonto", "Operaciones", Hash(destino)));
    }

    private static string Hash(string texto) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)));
    private string Ruta(string clave) => Path.Combine(directorio, Hash(clave) + ".json");
    private FileStream Bloquear(string clave) => new(Ruta(clave) + ".lock", FileMode.OpenOrCreate,
        FileAccess.ReadWrite, FileShare.None);
    private Registro? LeerRegistro(string clave) =>
        File.Exists(Ruta(clave)) ? JsonSerializer.Deserialize<Registro>(File.ReadAllText(Ruta(clave)))
            ?? throw new InvalidOperationException("Diario inválido; requiere conciliación.") : null;

    private void Guardar(string clave, Registro registro)
    {
        string temporal = Ruta(clave) + "." + Guid.NewGuid() + ".tmp";
        try
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(registro);
            using (var archivo = new FileStream(temporal, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                archivo.Write(bytes);
                archivo.Flush(flushToDisk: true);
            }
            File.Move(temporal, Ruta(clave), overwrite: true);
        }
        finally { if (File.Exists(temporal)) File.Delete(temporal); }
    }

    public T? Leer<T>(string clave) where T : class
    {
        using var bloqueo = Bloquear(clave);
        var registro = LeerRegistro(clave);
        return registro == null ? null : JsonSerializer.Deserialize<T>(registro.Solicitud)
            ?? throw new InvalidOperationException("Solicitud pendiente inválida.");
    }

    public Resultado<R> Ejecutar<T, R>(string clave, T solicitud, Func<Contexto, R> ejecutar)
    {
        using var bloqueo = Bloquear(clave);
        string json = JsonSerializer.Serialize(solicitud);
        Registro registro = LeerRegistro(clave) ?? new Registro(Guid.NewGuid(), json, null);
        if (registro.Id == Guid.Empty)
            throw new InvalidOperationException("Diario sin identificador válido; requiere conciliación.");
        if (registro.Solicitud != json)
            throw new InvalidOperationException("Existe una solicitud pendiente diferente. Reanudala antes de iniciar otra.");
        Guardar(clave, registro); // Obligatorio ANTES de cualquier llamada SQL.
        var contexto = new Contexto(registro, referencia =>
        {
            registro = registro with { Referencia = referencia, CreacionIniciada = false };
            Guardar(clave, registro);
        }, () =>
        {
            registro = registro with { CreacionIniciada = true };
            Guardar(clave, registro);
        });
        try { return new Resultado<R>(registro.Id, ejecutar(contexto)); }
        catch (SqlException ex) when (RechazoSinEscritura(ex.Number))
        {
            // No borrar el flujo de reserva: puede contener un pago ya aprobado.
            if (!clave.StartsWith("reserva-nueva:", StringComparison.Ordinal))
                Archivar(clave, registro.Id, ".rechazada");
            throw;
        }
    }

    public void Confirmar(string clave, Guid id)
    {
        using var bloqueo = Bloquear(clave);
        Archivar(clave, id, ".confirmada");
    }

    private void Archivar(string clave, Guid id, string sufijo)
    {
        if (LeerRegistro(clave)?.Id != id) return; // No completar otra operación concurrente.
        File.Move(Ruta(clave), Path.Combine(directorio, id + sufijo), overwrite: true);
    }

    private static bool RechazoSinEscritura(int numero) =>
        ErroresOperacionSql.Clasificar(numero) == HotelTramonto.Entidades.EstadoResultadoOperacion.RechazoDefinitivo;
}

public sealed record SolicitudCobro(int IdReserva, int IdUsuario, string Metodo, bool Servicios);
public sealed record SolicitudConsumo(int IdServicio, int IdReserva, int Cantidad, decimal Precio, int IdHabitacion);
public sealed record SolicitudReserva(int IdHuesped, int IdHabitacion, DateTime Entrada, DateTime Salida,
    string Modalidad, int Personas, int IdUsuario, decimal PagoInicial, string Metodo);
