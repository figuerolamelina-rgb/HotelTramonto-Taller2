using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public sealed record CatalogoHabitacionesFixture(int Tipo, int[] Habitaciones);

// Separación para probar planificación y colisiones sin abrir SQL.
// La implementación SQL solo inserta; nunca reutiliza ni modifica registros previos.
public interface ICatalogoHabitacionesFixture
{
    int? InsertarTipo(string nombre, decimal precioNoche, int capacidad);
    int? InsertarHabitacion(int tipo, string numero);
    void Validar(CatalogoHabitacionesFixture creado);
}

public static class HabitacionesFixture
{
    public const decimal PrecioNoche = 100m;
    public const int Capacidad = 4;
    public const int Cantidad = 3;
    private const int MaxIntentos = 32;

    public static DateTime EntradaTemporal(int secuencia)
    {
        if (secuencia < 0) throw new ArgumentOutOfRangeException(nameof(secuencia));
        // Cada caso tiene habitaciones propias: intervalos de dos noches separados
        // por tres días evitan colisiones aleatorias entre las temporales del mismo caso.
        return new DateTime(2300, 1, 1).AddDays(checked(secuencia * 3));
    }

    public static CatalogoHabitacionesFixture Preparar(ICatalogoHabitacionesFixture datos, Func<Guid>? nuevoId = null)
    {
        ArgumentNullException.ThrowIfNull(datos);
        nuevoId ??= Guid.NewGuid;
        int? tipo = null;
        for (int i = 0; i < MaxIntentos && tipo is null; i++)
            tipo = datos.InsertarTipo("HT56-" + nuevoId().ToString("N"), PrecioNoche, Capacidad);
        if (tipo is null || tipo <= 0) throw new InvalidOperationException("No se pudo crear un tipo exclusivo para el fixture.");

        var habitaciones = new List<int>();
        for (int indice = 0; indice < Cantidad; indice++)
        {
            int? habitacion = null;
            for (int intento = 0; intento < MaxIntentos && habitacion is null; intento++)
            {
                // VARCHAR(10): F + nueve dígitos hexadecimales. La unicidad se comprueba en SQL,
                // no se presupone por la probabilidad del GUID.
                string numero = "F" + nuevoId().ToString("N")[..9];
                habitacion = datos.InsertarHabitacion(tipo.Value, numero);
            }
            if (habitacion is null || habitacion <= 0 || habitaciones.Contains(habitacion.Value))
                throw new InvalidOperationException("No se obtuvieron tres habitaciones nuevas y distintas.");
            habitaciones.Add(habitacion.Value);
        }
        var creado = new CatalogoHabitacionesFixture(tipo.Value, habitaciones.ToArray());
        datos.Validar(creado);
        return creado;
    }

    public static CatalogoHabitacionesFixture Crear(CopiaSql copia)
    {
        using var conexion = copia.Abrir(); // Nombre GUID y marca propios, ANTES de cualquier INSERT.
        using var transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            var creado = Preparar(new Datos(conexion, transaccion));
            transaccion.Commit(); // Los procedimientos probados necesitan transacciones independientes.
            return creado;
        }
        catch (Exception original)
        {
            try { if (transaccion.Connection is not null) transaccion.Rollback(); }
            catch (Exception rollback) when (rollback is SqlException or InvalidOperationException)
            {
                // Conexión perdida o transacción ya revertida: conservar el error que provocó el fallo.
                original.Data["FalloRollbackFixture"] = rollback.Message;
            }
            throw; // Un fallo de FK/CHECK/permisos/preparación nunca se informa como prueba aprobada.
        }
    }

    private sealed class Datos(SqlConnection conexion, SqlTransaction transaccion) : ICatalogoHabitacionesFixture
    {
        public int? InsertarTipo(string nombre, decimal precioNoche, int capacidad)
        {
            using var cmd = Sql.Comando(conexion, """
                INSERT dbo.TiposHabitacion(nombre,descripcion,precio_noche,capacidad,activo)
                OUTPUT inserted.id_tipoHabitacion
                SELECT @p0,'Fixture exclusivo de habitaciones correcciones 5/6',@p1,@p2,1
                WHERE NOT EXISTS(SELECT 1 FROM dbo.TiposHabitacion WITH (UPDLOCK,HOLDLOCK) WHERE nombre=@p0);
                """, nombre, precioNoche, capacidad);
            cmd.Transaction = transaccion;
            object? resultado = cmd.ExecuteScalar();
            return resultado is null or DBNull ? null : Convert.ToInt32(resultado);
        }

        public int? InsertarHabitacion(int tipo, string numero)
        {
            using var cmd = Sql.Comando(conexion, """
                INSERT dbo.Habitaciones(tipo_habitacion,piso,numero,estado,activo)
                OUTPUT inserted.id_habitacion
                SELECT @p0,NULL,@p1,'DISPONIBLE',1
                WHERE NOT EXISTS(SELECT 1 FROM dbo.Habitaciones WITH (UPDLOCK,HOLDLOCK) WHERE numero=@p1);
                """, tipo, numero);
            cmd.Transaction = transaccion;
            object? resultado = cmd.ExecuteScalar();
            return resultado is null or DBNull ? null : Convert.ToInt32(resultado);
        }

        public void Validar(CatalogoHabitacionesFixture creado)
        {
            using var cmd = Sql.Comando(conexion, """
                SELECT COUNT(*) FROM dbo.Habitaciones h JOIN dbo.TiposHabitacion t
                 ON t.id_tipoHabitacion=h.tipo_habitacion
                WHERE h.id_habitacion IN (@p0,@p1,@p2) AND h.tipo_habitacion=@p3
                 AND h.activo=1 AND h.estado='DISPONIBLE' AND t.activo=1
                 AND t.precio_noche=@p4 AND t.capacidad=@p5;
                """, creado.Habitaciones[0], creado.Habitaciones[1], creado.Habitaciones[2],
                creado.Tipo, PrecioNoche, Capacidad);
            cmd.Transaction = transaccion;
            if (Convert.ToInt32(cmd.ExecuteScalar()) != Cantidad)
                throw new InvalidOperationException("El catálogo nuevo del fixture no cumple las restricciones.");
        }
    }
}
