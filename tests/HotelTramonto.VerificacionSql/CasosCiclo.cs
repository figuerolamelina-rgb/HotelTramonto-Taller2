using Microsoft.Data.SqlClient;

namespace HotelTramonto.VerificacionSql;

public sealed partial class CasosSql
{
    public static readonly (string Nombre, Action<CasosSql> Prueba)[] Ciclo =
    [
        ("29_CheckinContraCancelacion", c => c.CheckinCancelacion()),
        ("30_CheckinContraCierre", c => c.CheckinCierre()),
        ("31_DosCheckin", c => c.DosCheckin()),
        ("32_CheckinDemoradoNoReabreCierre", c => c.TransicionDemorada(true)),
        ("33_CancelacionDemoradaNoCambiaCheckin", c => c.TransicionDemorada(false)),
        ("34_ConsumoContraCheckin", c => c.ConsumoCheckin()),
        ("35_CierreRechazaCancelada", c => c.CierreCancelada()),
        ("36_CheckinMarcaCierreYDeudaCentavo", c => c.CheckinMarcaCierre())
    ];

    private void CheckinCancelacion()
    {
        int r = Reserva("CONFIRMADA", 1);
        var resultados = Paralelo(c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckIn @p0", r),
            c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_Cancelar @p0", r));
        string estado = (string)Sql.Escalar(db, "SELECT estado FROM dbo.Reservas WHERE id_reserva=@p0", r)!;
        Exigir(estado == "CHECKIN" ? resultados.SequenceEqual(new[] { 0, 50042 }) :
            estado == "CANCELADA" && resultados.SequenceEqual(new[] { 50027, 0 }));
    }

    private void CheckinCierre()
    {
        int r = Reserva("CONFIRMADA", 1);
        var resultados = Paralelo(c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckIn @p0", r),
            c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckOut @p0", r));
        string estado = (string)Sql.Escalar(db, "SELECT estado FROM dbo.Reservas WHERE id_reserva=@p0", r)!;
        Exigir(estado == "FINALIZADA" ? resultados.SequenceEqual(new[] { 0, 0 }) :
            estado == "CHECKIN" && resultados.SequenceEqual(new[] { 0, 50030 }));
    }

    private void DosCheckin()
    {
        int r = Reserva("CONFIRMADA", 1);
        Resultados(Paralelo(c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckIn @p0", r),
            c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckIn @p0", r)), 0, 50027);
    }

    private void TransicionDemorada(bool checkin)
    {
        int r = Reserva("CONFIRMADA", 1);
        using var espera = copia.Abrir();
        using var observador = copia.Abrir();
        int sesion = Convert.ToInt32(Sql.Escalar(espera, "SELECT @@SPID"));
        int titular = Convert.ToInt32(Sql.Escalar(db, "SELECT @@SPID"));
        Task<int>? tarea = null;
        int? codigo;
        try
        {
            using var transaccion = db.BeginTransaction();
            using var bloqueo = Sql.Comando(db, """
                DECLARE @resultado int,@recurso nvarchar(255)=CONCAT(N'HotelTramonto:reserva:',@p0);
                EXEC @resultado=sys.sp_getapplock @Resource=@recurso,@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
                IF @resultado<0 THROW 51720,'No se obtuvo bloqueo de prueba.',1;
                """, r);
            bloqueo.Transaction = transaccion;
            bloqueo.ExecuteNonQuery();
            tarea = Task.Run(() =>
            {
                try
                {
                    Sql.Ejecutar(espera, checkin ? "EXEC dbo.sp_Reserva_CheckIn @p0" : "EXEC dbo.sp_Reserva_Cancelar @p0", r);
                    return 0;
                }
                catch (SqlException ex) { return ex.Number; }
            });
            // Acredita espera de bloqueo real; no se considera suficiente lanzar dos tareas.
            var limite = DateTime.UtcNow.AddSeconds(5);
            bool bloqueada = false;
            while (DateTime.UtcNow < limite && !tarea.IsCompleted)
            {
                bloqueada = Convert.ToInt32(Sql.Escalar(observador,
                    "SELECT COUNT(*) FROM sys.dm_exec_requests WHERE session_id=@p0 AND blocking_session_id=@p1 AND wait_type LIKE 'LCK[_]M[_]%'",
                    sesion, titular)) == 1;
                if (bloqueada) break;
                Thread.Sleep(20);
            }
            Exigir(bloqueada, "No se observó espera real; revisar permisos VIEW SERVER STATE/PERFORMANCE STATE.");
            using var actualizar = Sql.Comando(db, checkin ?
                "UPDATE dbo.Reservas SET estado='FINALIZADA',fecha_checkout=GETDATE() WHERE id_reserva=@p0;" :
                "UPDATE dbo.Reservas SET estado='CHECKIN',fecha_checkin=GETDATE() WHERE id_reserva=@p0;", r);
            actualizar.Transaction = transaccion;
            actualizar.ExecuteNonQuery();
            transaccion.Commit();
        }
        finally
        {
            // Al salir del try, SqlTransaction ya confirmó o revirtió y liberó el bloqueo.
            codigo = tarea?.GetAwaiter().GetResult();
        }
        int esperado = checkin ? 50027 : 50042;
        Exigir(codigo == esperado, $"Se esperaba el rechazo SQL {esperado} y se obtuvo {codigo}.");
        string estado = (string)Sql.Escalar(db, "SELECT estado FROM dbo.Reservas WHERE id_reserva=@p0", r)!;
        Exigir(estado == (checkin ? "FINALIZADA" : "CHECKIN"), $"Estado inesperado tras la espera: {estado}.");
    }

    private void ConsumoCheckin()
    {
        int r = Reserva("CONFIRMADA", 1);
        var resultados = Paralelo(c => Sql.Ejecutar(c, "EXEC dbo.sp_Reserva_CheckIn @p0", r),
            c => Consumir(c, r: r));
        Exigir(resultados[0] == 0 && resultados[1] is 0 or 51315);
        Exigir(Convert.ToInt32(Sql.Escalar(db, "SELECT COUNT(*) FROM dbo.DetalleServicios WHERE id_reserva=@p0", r)) ==
            (resultados[1] == 0 ? 1 : 0));
    }

    private void CierreCancelada()
    {
        int r = Reserva("CONFIRMADA", 1);
        Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_Cancelar @p0", r);
        Error(50030, () => Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckOut @p0", r));
        Error(50027, () => Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckIn @p0", r));
    }

    private void CheckinMarcaCierre()
    {
        int r = Reserva("CONFIRMADA", 1);
        Sql.Ejecutar(db, "UPDATE dbo.Reservas SET total=0.01 WHERE id_reserva=@p0", r);
        Error(50028, () => Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckIn @p0", r));
        Sql.Ejecutar(db, "UPDATE dbo.Reservas SET fecha_checkout=GETDATE() WHERE id_reserva=@p0", r);
        Error(50027, () => Sql.Ejecutar(db, "EXEC dbo.sp_Reserva_CheckIn @p0", r));
    }
}
