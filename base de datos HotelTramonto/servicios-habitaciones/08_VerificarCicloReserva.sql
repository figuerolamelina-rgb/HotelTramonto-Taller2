-- Actualización NUEVA. No reinstala 01 ni 04. No ejecutar automáticamente.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51700, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
IF COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')),N'')<>N'LAPTOP-5TSJJUP8'
 OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')),N'')<>N'SQLEXPRESS'
 THROW 51701, 'Instancia no autorizada.', 1;
IF @@TRANCOUNT<>0 THROW 51702, 'Ejecutar sin transaccion externa.', 1;
DECLARE @actual0 nvarchar(max)=OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_Reserva_CheckIn')), @nuevo0 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckIn
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51312, ''No se admite transaccion externa.'', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @bloqueo INT, @recurso NVARCHAR(255)=CONCAT(N''HotelTramonto:reserva:'',@id_reserva);
        EXEC @bloqueo=sys.sp_getapplock @Resource=@recurso,
            @LockMode=''Exclusive'', @LockOwner=''Transaction'', @LockTimeout=10000;
        IF @bloqueo<0 THROW 51310, ''No se pudo bloquear la reserva. Reintente.'', 1;
        DECLARE @estado VARCHAR(20), @total DECIMAL(10,2), @pagado DECIMAL(38,2), @cierre DATETIME;
        SELECT @estado=estado,@total=total,@cierre=fecha_checkout
        FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK) WHERE id_reserva=@id_reserva;
        IF @estado IS NULL THROW 50026, ''La reserva no existe.'', 1;
        IF @estado<>''CONFIRMADA'' OR @cierre IS NOT NULL
            THROW 50027, ''La reserva debe estar CONFIRMADA y sin cierre para realizar el check-in.'', 1;
        SELECT @pagado=COALESCE(SUM(CONVERT(DECIMAL(38,2),monto)),0)
        FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
        WHERE id_reserva=@id_reserva AND estado=''APROBADO''
          AND tipo_pago IN (''PAGO_INICIAL'',''SALDO_ALOJAMIENTO'');
        IF @total>@pagado
            THROW 50028, ''No se puede realizar el check-in porque existe saldo de alojamiento pendiente.'', 1;
        UPDATE dbo.Reservas SET estado=''CHECKIN'',fecha_checkin=GETDATE()
        WHERE id_reserva=@id_reserva AND estado=''CONFIRMADA'' AND fecha_checkout IS NULL;
        IF @@ROWCOUNT<>1 THROW 50027, ''La reserva cambio de estado.'', 1;
        COMMIT TRANSACTION;
        SELECT id_reserva,estado,fecha_checkin FROM dbo.Reservas WHERE id_reserva=@id_reserva;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';
IF @actual0 IS NULL OR LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@nuevo0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2
 THROW 51706, 'Definicion de ciclo distinta de la revisada.', 1;
DECLARE @actual1 nvarchar(max)=OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_Reserva_Cancelar')), @nuevo1 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Cancelar
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51312, ''No se admite transaccion externa.'', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @bloqueo INT, @recurso NVARCHAR(255)=CONCAT(N''HotelTramonto:reserva:'',@id_reserva);
        EXEC @bloqueo=sys.sp_getapplock @Resource=@recurso,
            @LockMode=''Exclusive'', @LockOwner=''Transaction'', @LockTimeout=10000;
        IF @bloqueo<0 THROW 51310, ''No se pudo bloquear la reserva. Reintente.'', 1;
        DECLARE @estado VARCHAR(20), @cierre DATETIME;
        SELECT @estado=estado,@cierre=fecha_checkout FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK)
        WHERE id_reserva=@id_reserva;
        IF @estado IS NULL THROW 50040, ''La reserva no existe.'', 1;
        IF @estado=''CANCELADA'' THROW 50041, ''La reserva ya esta cancelada.'', 1;
        IF @estado<>''CONFIRMADA'' OR @cierre IS NOT NULL
            THROW 50042, ''Solo se pueden cancelar reservas confirmadas y sin cierre.'', 1;
        UPDATE dbo.Reservas SET estado=''CANCELADA''
        WHERE id_reserva=@id_reserva AND estado=''CONFIRMADA'' AND fecha_checkout IS NULL;
        IF @@ROWCOUNT<>1 THROW 50042, ''La reserva cambio de estado.'', 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';
IF @actual1 IS NULL OR LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@nuevo1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2
 THROW 51706, 'Definicion de ciclo distinta de la revisada.', 1;
SELECT N'Definiciones de check-in y cancelacion verificadas; concurrencia no ejecutada por esta consulta' AS resultado;
