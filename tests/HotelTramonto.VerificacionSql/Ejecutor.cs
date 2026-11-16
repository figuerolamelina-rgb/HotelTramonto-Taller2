using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public sealed class ResultadoCaso(string nombre)
{
    public string Nombre { get; set; } = nombre;
    public string Estado { get; set; } = "Pendiente";
    public string? Detalle { get; set; }
}
public sealed class Informe
{
    public Guid Ejecucion { get; set; }
    public string BaseTemporal { get; set; } = "";
    public string Etapa { get; set; } = "No iniciada";
    public string Historicos { get; set; } = "Pendiente";
    public string Limpieza { get; set; } = "No creada";
    public string? Error { get; set; }
    public List<ResultadoCaso> Pruebas { get; set; } = CasosSql.Todos.Select(c => new ResultadoCaso(c.Nombre)).ToList();
    public int Aprobadas => Pruebas.Count(p => p.Estado == "Aprobada");
    public int Fallidas => Pruebas.Count(p => p.Estado == "Fallida");
    public int Pendientes => Pruebas.Count(p => p.Estado == "Pendiente");
}

public sealed class Ejecutor(string raiz, string backup)
{
    private readonly IdentidadCopia identidad = new(Guid.NewGuid());
    private CopiaSql Copia => new(identidad);
    private string CarpetaSql => Path.Combine(raiz, "base de datos HotelTramonto", "servicios-habitaciones");
    private int? idCreado;
    private DateTime? fechaCreada;
    private byte[]? propietario;
    private readonly List<string> archivos = [];
    private bool restauracionIntentada;
    private bool marcada;
    private string DirectorioInformes => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HotelTramonto", "VerificacionesExpress", identidad.Ejecucion.ToString("N"));

    public int Ejecutar()
    {
        var informe = new Informe { Ejecucion = identidad.Ejecucion, BaseTemporal = identidad.Nombre };
        Directory.CreateDirectory(DirectorioInformes); // Antes de cualquier SQL.
        try
        {
            if (!OperatingSystem.IsWindows() ||
                !Environment.MachineName.Equals("LAPTOP-5TSJJUP8", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Solo se admite la instancia Express local autorizada en LAPTOP-5TSJJUP8.");
            if (!File.Exists(backup) || !Path.GetExtension(backup).Equals(".bak", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Falta el respaldo FULL .bak posterior a categoria y anterior a correcciones 5/6.");
            informe.Etapa = "Validación de motor y respaldo";
            PrepararCopia();
            informe.Limpieza = "Pendiente";
            informe.Etapa = "Diagnóstico";
            Migrar("00_Diagnostico.sql");
            var historicos = Historicos.Capturar(Copia);
            Console.WriteLine($"Destino ÚNICO: {identidad.Nombre}, ejecución {identidad.Ejecucion:D}.");
            Console.WriteLine("Se instalará 01 -> 02 -> 04 -> 05 SOLO en la copia temporal.");
            Console.WriteLine("HotelTramontoDB y HotelTramontoDB_Pruebas no se abrirán ni modificarán.");
            Console.Write("Escribí INSTALAR para autorizar la migración de esta copia: ");
            if (Console.ReadLine() != "INSTALAR")
                throw new OperationCanceledException("No se autorizó instalar; las 28 pruebas quedan pendientes.");
            informe.Etapa = "Instalación";
            foreach (string nombre in new[] { "01_Instalar.sql", "02_Verificar.sql", "04_ConcurrenciaPagos.sql", "05_VerificarConcurrencia.sql" })
                Migrar(nombre);
            try { historicos.Verificar(Copia); }
            catch { informe.Historicos = "FALLIDA: se detectó un cambio histórico"; throw; }
            informe.Etapa = "28 pruebas SQL";
            for (int i = 0; i < CasosSql.Todos.Length; i++)
            {
                var (nombre, accion) = CasosSql.Todos[i];
                try
                {
                    using var caso = new CasosSql(Copia);
                    accion(caso);
                    informe.Pruebas[i].Estado = "Aprobada";
                }
                catch (Exception ex)
                {
                    informe.Pruebas[i].Estado = "Fallida";
                    informe.Pruebas[i].Detalle = Describir(ex);
                }
                Console.WriteLine($"{nombre}: {informe.Pruebas[i].Estado}");
            }
            informe.Etapa = "Verificación final";
            try { historicos.Verificar(Copia); }
            catch { informe.Historicos = "FALLIDA: se detectó un cambio histórico"; throw; }
            informe.Historicos = "Once tablas: filas y columnas preexistentes intactas";
            Migrar("02_Verificar.sql");
            Migrar("05_VerificarConcurrencia.sql");
            informe.Etapa = "Completada";
        }
        catch (Exception ex) { informe.Error = Describir(ex); }
        finally
        {
            if (restauracionIntentada)
            {
                try { EliminarCopia(); informe.Limpieza = "Eliminada: identidad y archivos propios comprobados"; }
                catch (Exception ex) { informe.Limpieza = "NO CONFIRMADA; no se reintenta eliminar: " + Describir(ex); }
            }
            string json = JsonSerializer.Serialize(informe, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(DirectorioInformes, "informe.json"), json);
            File.WriteAllText(Path.Combine(DirectorioInformes, "informe.txt"),
                $"Base temporal: {informe.BaseTemporal}\nEtapa: {informe.Etapa}\n" +
                $"Aprobadas: {informe.Aprobadas}; fallidas: {informe.Fallidas}; pendientes: {informe.Pendientes}\n" +
                $"Históricos: {informe.Historicos}\nLimpieza: {informe.Limpieza}\nError: {informe.Error}\n" +
                string.Join("\n", informe.Pruebas.Select(p => $"{p.Nombre}: {p.Estado} {p.Detalle}")));
            Console.WriteLine($"Aprobadas: {informe.Aprobadas}; fallidas: {informe.Fallidas}; pendientes: {informe.Pendientes}.");
            Console.WriteLine("Históricos: " + informe.Historicos);
            Console.WriteLine("Limpieza: " + informe.Limpieza);
            Console.WriteLine("Informes privados: " + DirectorioInformes);
            if (informe.Error != null) Console.WriteLine("Error: " + informe.Error);
        }
        return informe.Error == null && informe.Aprobadas == 28 && informe.Fallidas == 0 &&
            informe.Pendientes == 0 && informe.Limpieza.StartsWith("Eliminada:", StringComparison.Ordinal) ? 0 : 1;
    }

    private static string Describir(Exception ex) =>
        ex is SqlException sql ? $"SQL {sql.Number}: {sql.Message}" : ex.GetType().Name + ": " + ex.Message;

    private void PrepararCopia()
    {
        using var master = Copia.AbrirSinMarca("master");
        var motor = Sql.Tabla(master, """
            SELECT CONVERT(int,SERVERPROPERTY('EngineEdition')) AS edicion,
             CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')) AS equipo,
             CONVERT(int,SERVERPROPERTY('ProductMajorVersion')) AS version,
             CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')) AS instancia,
             CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultDataPath')) AS datos,
             CONVERT(nvarchar(4000),SERVERPROPERTY('InstanceDefaultLogPath')) AS logs;
            """).Rows[0];
        if ((int)motor["edicion"] != 4 || !((string)motor["equipo"]).Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) ||
            !((string)motor["instancia"]).Equals("SQLEXPRESS", StringComparison.OrdinalIgnoreCase) ||
            (int)motor["version"] < 14)
            throw new InvalidOperationException("Se requiere SQL Server Express local 2017 o posterior.");
        Sql.Ejecutar(master, """
            IF COALESCE(HAS_PERMS_BY_NAME(NULL,NULL,'VIEW ANY DEFINITION'),0)<>1
             THROW 51607,'No se puede acreditar visibilidad completa de metadatos del servidor.',1;
            IF CONVERT(int,SERVERPROPERTY('ProductMajorVersion'))>=16
            BEGIN
             IF COALESCE(HAS_PERMS_BY_NAME(NULL,NULL,'VIEW SERVER PERFORMANCE STATE'),0)<>1
              THROW 51608,'Falta visibilidad de sesiones para limpieza segura.',1;
            END
            ELSE IF COALESCE(HAS_PERMS_BY_NAME(NULL,NULL,'VIEW SERVER STATE'),0)<>1
             THROW 51608,'Falta visibilidad de sesiones para limpieza segura.',1;
            IF EXISTS(SELECT 1 FROM sys.server_triggers WHERE is_ms_shipped=0 AND is_disabled=0)
             THROW 51606,'Triggers de servidor activos: revisión manual antes de restaurar.',1;
            """);
        if (Sql.Escalar(master, "SELECT DB_ID(@p0)", identidad.Nombre) is not DBNull)
            throw new InvalidOperationException("El nombre temporal ya existe; no se sobrescribe.");
        var header = Sql.Tabla(master, "RESTORE HEADERONLY FROM DISK=@p0", backup);
        if (header.Rows.Count != 1 || (string)header.Rows[0]["DatabaseName"] != "HotelTramontoDB_Pruebas" ||
            Convert.ToInt32(header.Rows[0]["BackupType"]) != 1 ||
            Convert.ToInt32(header.Rows[0]["SoftwareVersionMajor"]) > (int)motor["version"])
            throw new InvalidOperationException("El archivo debe contener un único FULL compatible de HotelTramontoDB_Pruebas.");
        var lista = Sql.Tabla(master, "RESTORE FILELISTONLY FROM DISK=@p0", backup);
        if (lista.Rows.Count < 2) throw new InvalidOperationException("Respaldo sin archivos esperados.");
        var clausulas = new List<string>();
        for (int i = 0; i < lista.Rows.Count; i++)
        {
            var fila = lista.Rows[i];
            string tipo = (string)fila["Type"];
            if (tipo != "D" && tipo != "L") throw new InvalidOperationException("Solo se soportan archivos de datos y log ordinarios.");
            string destino = Path.Combine((string)motor[tipo == "L" ? "logs" : "datos"],
                "HT56_" + identidad.Ejecucion.ToString("N") + "_" + i + (tipo == "L" ? ".ldf" : ".mdf"));
            archivos.Add(destino);
            // Se verifica existencia en el servidor; no se usa File.Exists del cliente.
            var existe = Sql.Tabla(master, "EXEC master.dbo.xp_fileexist @p0", destino).Rows[0];
            if (Convert.ToInt32(existe[0]) != 0 || Convert.ToInt32(existe[1]) != 0 || Convert.ToInt32(existe[2]) != 1)
                throw new InvalidOperationException("Archivo ya existente o directorio SQL inaccesible; se cancela.");
            Sql.Ejecutar(master, "IF EXISTS(SELECT 1 FROM sys.master_files WHERE physical_name=@p0) THROW 51601,'Archivo en uso.',1;", destino);
            clausulas.Add("MOVE N'" + ((string)fila["LogicalName"]).Replace("'", "''") + "' TO N'" + destino.Replace("'", "''") + "'");
        }
        // El comando comprueba otra vez el nombre. No existe WITH REPLACE ni opción equivalente.
        restauracionIntentada = true;
        Sql.Ejecutar(master, """
            IF DB_ID(@p1) IS NOT NULL THROW 51602,'La base ya existe; no se restaura.',1;
            """ + "\nRESTORE DATABASE " + IdentidadCopia.Identificador(identidad.Nombre) +
            " FROM DISK=@p0 WITH " + string.Join(",", clausulas) + ", RECOVERY;", backup, identidad.Nombre);
        var metadata = Sql.Tabla(master, "SELECT database_id,create_date,owner_sid FROM sys.databases WHERE name=@p0", identidad.Nombre).Rows[0];
        idCreado = (int)metadata["database_id"];
        fechaCreada = (DateTime)metadata["create_date"];
        propietario = (byte[])metadata["owner_sid"];
        ExigirArchivos(master);
        using var copia = Copia.AbrirSinMarca(identidad.Nombre);
        Sql.Ejecutar(copia, """
            IF EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'HotelTramonto_TestRun')
             THROW 51603,'Respaldo ya marcado: no corresponde a una instalación inicial.',1;
            IF EXISTS(SELECT 1 FROM sys.triggers WHERE is_ms_shipped=0 AND is_disabled=0)
             OR EXISTS(SELECT 1 FROM sys.synonyms)
             OR EXISTS(SELECT 1 FROM sys.assemblies WHERE is_user_defined=1)
             OR EXISTS(SELECT 1 FROM sys.databases WHERE database_id=DB_ID() AND
                (is_trustworthy_on=1 OR compatibility_level<130))
             THROW 51604,'La copia requiere revisión de automatismos o compatibilidad.',1;
            EXEC sys.sp_addextendedproperty @name=N'HotelTramonto_TestRun',@value=@p0;
            """, identidad.Ejecucion.ToString("D"));
        marcada = true;
        using var comprobada = Copia.Abrir();
        foreach (string tabla in ContratosBase.Tablas)
            if (Convert.ToInt32(Sql.Escalar(comprobada, "SELECT COUNT(*) FROM sys.tables WHERE schema_id=SCHEMA_ID('dbo') AND name=@p0", tabla)) != 1)
                throw new InvalidOperationException("No se acepta vista o tabla ausente: " + tabla);
        foreach (string nombre in ContratosBase.Procedimientos)
        {
            string? definicion = Sql.Escalar(comprobada, "SELECT OBJECT_DEFINITION(OBJECT_ID(@p0,'P'))", "dbo." + nombre) as string;
            if (definicion is null || ContratosBase.Canonizar(definicion, nombre) != ContratosBase.Esperado(raiz, nombre))
                throw new InvalidOperationException("Procedimiento de base diferente al revisado: " + nombre + ". No se ejecutará.");
        }
        // Primer respaldo anterior a 5/6: no se quitan migraciones ya instaladas.
        Sql.Ejecutar(comprobada, """
            IF COL_LENGTH('dbo.Pagos','id_operacion') IS NOT NULL
             OR COL_LENGTH('dbo.DetalleServicios','id_habitacion') IS NOT NULL
             THROW 51605,'El respaldo ya contiene correcciones 5/6.',1;
            """);
    }

    private void ExigirArchivos(SqlConnection master)
    {
        var filas = Sql.Tabla(master, "SELECT physical_name FROM sys.master_files WHERE database_id=DB_ID(@p0)", identidad.Nombre);
        var reales = filas.Rows.Cast<DataRow>().Select(r => (string)r[0]).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!reales.SetEquals(archivos)) throw new InvalidOperationException("No coinciden los archivos propios de esta ejecución.");
    }

    private void Migrar(string nombre)
    {
        if (nombre == "05_VerificarConcurrencia.sql")
            DiagnosticoDefiniciones.Guardar(Copia, raiz, DirectorioInformes);
        string original = File.ReadAllText(Path.Combine(CarpetaSql, nombre));
        using var db = Copia.Abrir();
        using var cmd = new SqlCommand(identidad.Adaptar(original), db) { CommandTimeout = 300 };
        cmd.Parameters.Add("@HT_Run", SqlDbType.UniqueIdentifier).Value = identidad.Ejecucion;
        cmd.Parameters.Add("@HT_Db", SqlDbType.NVarChar, 128).Value = identidad.Nombre;
        using var adapter = new SqlDataAdapter(cmd);
        var resultados = new DataSet();
        adapter.Fill(resultados);
        resultados.WriteXml(Path.Combine(DirectorioInformes, nombre + ".xml"), XmlWriteMode.WriteSchema);
    }

    private void EliminarCopia()
    {
        if (!marcada || idCreado is null || fechaCreada is null || propietario is null)
            throw new InvalidOperationException("No se acreditó identidad completa. Conservar la copia para revisión manual.");
        using (var validada = Copia.Abrir()) { } // Comprueba marca antes de abrir la conexión de borrado.
        using var master = Copia.AbrirSinMarca("master");
        ExigirArchivos(master);
        string nombre = IdentidadCopia.Identificador(identidad.Nombre);
        string propiedad = nombre + ".sys.extended_properties";
        // NO_WAIT: jamás expulsa sesiones ni revierte transacciones ajenas.
        Sql.Ejecutar(master, """
            SET XACT_ABORT ON;
            BEGIN TRY
             IF NOT EXISTS(SELECT 1 FROM sys.databases WHERE name=@p0 COLLATE Latin1_General_100_BIN2
              AND database_id=@p1 AND create_date=@p2 AND owner_sid=@p3)
              THROW 51610,'Cambio de identidad: no se elimina.',1;
            """ + "\n IF NOT EXISTS(SELECT 1 FROM " + propiedad + """
             WHERE class=0 AND name=N'HotelTramonto_TestRun' AND CONVERT(nvarchar(36),value)=@p4)
              THROW 51611,'Marca no coincide: no se elimina.',1;
             IF EXISTS(SELECT 1 FROM sys.dm_exec_sessions WHERE database_id=@p1 AND session_id<>@@SPID)
              THROW 51612,'Hay otras sesiones en la copia. No se elimina.',1;
            """ + "\n ALTER DATABASE " + nombre + " SET SINGLE_USER WITH NO_WAIT;\n" +
            " IF DB_ID(@p0)<>@p1 THROW 51613,'Cambio de identidad durante limpieza.',1;\n" +
            " IF NOT EXISTS(SELECT 1 FROM " + propiedad +
            " WHERE class=0 AND name=N'HotelTramonto_TestRun' AND CONVERT(nvarchar(36),value)=@p4)" +
            " THROW 51614,'Cambio de marca durante limpieza.',1;\n DROP DATABASE " + nombre + """
            ;
            END TRY
            BEGIN CATCH
             THROW;
            END CATCH;
            """, identidad.Nombre, idCreado, fechaCreada, propietario, identidad.Ejecucion.ToString("D"));
    }
}
