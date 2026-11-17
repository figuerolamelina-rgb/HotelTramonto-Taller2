using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public static class Sql
{
    public static SqlCommand Comando(SqlConnection conexion, string texto, params object?[] valores)
    {
        var cmd = new SqlCommand(texto, conexion) { CommandTimeout = 300 };
        for (int i = 0; i < valores.Length; i++)
        {
            var valor = valores[i];
            var p = new SqlParameter("@p" + i, valor ?? DBNull.Value);
            if (valor is decimal) { p.SqlDbType = SqlDbType.Decimal; p.Precision = 18; p.Scale = 2; }
            if (valor is Guid) p.SqlDbType = SqlDbType.UniqueIdentifier;
            cmd.Parameters.Add(p);
        }
        return cmd;
    }

    public static DataTable Tabla(SqlConnection conexion, string texto, params object?[] valores)
    {
        using var cmd = Comando(conexion, texto, valores);
        using var adapter = new SqlDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static object? Escalar(SqlConnection conexion, string texto, params object?[] valores)
    {
        using var cmd = Comando(conexion, texto, valores);
        return cmd.ExecuteScalar();
    }

    public static void Ejecutar(SqlConnection conexion, string texto, params object?[] valores)
    {
        using var cmd = Comando(conexion, texto, valores);
        cmd.ExecuteNonQuery();
    }
}

public sealed class CopiaSql
{
    public IdentidadCopia Identidad { get; }
    public CopiaSql(IdentidadCopia identidad) { IdentidadCopia.ExigirNueva(identidad.Ejecucion); Identidad = identidad; }

    internal SqlConnection AbrirSinMarca(string baseDatos)
    {
        if (baseDatos != "master" && baseDatos != Identidad.Nombre)
            throw new InvalidOperationException("Base fuera del ámbito de la copia.");
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "lpc:" + IdentidadCopia.Servidor, InitialCatalog = baseDatos,
            IntegratedSecurity = true, Pooling = false, ConnectTimeout = 20,
            Encrypt = SqlConnectionEncryptOption.Optional, TrustServerCertificate = false,
            ApplicationName = "HotelTramonto.VerificacionSql." + Identidad.Ejecucion.ToString("N")
        };
        var conexion = new SqlConnection(builder.ConnectionString);
        try { conexion.Open(); return conexion; }
        catch { conexion.Dispose(); throw; }
    }

    public SqlConnection Abrir()
    {
        var conexion = AbrirSinMarca(Identidad.Nombre);
        try
        {
            var metadata = Sql.Tabla(conexion, """
                SELECT DB_NAME() AS nombre,
                 (SELECT CONVERT(nvarchar(36),value) FROM sys.extended_properties
                  WHERE class=0 AND name=N'HotelTramonto_TestRun') AS marca;
                """).Rows[0];
            if (!Identidad.EsPropia((string)metadata["nombre"], metadata["marca"] as string))
                throw new InvalidOperationException("La copia no pertenece a esta ejecución.");
            return conexion;
        }
        catch { conexion.Dispose(); throw; }
    }
}
