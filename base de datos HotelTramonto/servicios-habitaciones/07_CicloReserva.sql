-- Actualización NUEVA. No reinstala 01 ni 04. No ejecutar automáticamente.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51700, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
IF COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')),N'')<>N'LAPTOP-5TSJJUP8'
 OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')),N'')<>N'SQLEXPRESS'
 THROW 51701, 'Instancia no autorizada.', 1;
IF @@TRANCOUNT<>0 THROW 51702, 'Ejecutar sin transaccion externa.', 1;
SET XACT_ABORT ON;
BEGIN TRY
 BEGIN TRANSACTION;
 IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE major_id IN
    (OBJECT_ID(N'dbo.sp_Reserva_CheckIn'),OBJECT_ID(N'dbo.sp_Reserva_Cancelar')))
  THROW 51703, 'Modulos firmados: coordinar con DBA antes de actualizar.', 1;
 IF COL_LENGTH(N'dbo.Pagos',N'id_operacion') IS NULL OR
    COALESCE(OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_Reserva_CheckOut')),N'') NOT LIKE N'%HotelTramonto:reserva:%'
  THROW 51704, 'Falta el protocolo de pagos/cierre de 04; no se reinstala automaticamente.', 1;
DECLARE @actual0 nvarchar(max)=OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_Reserva_CheckIn')), @anterior0 nvarchar(max)=N'CREATE OR ALTER PROCEDURE sp_Reserva_CheckIn
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @total_alojamiento DECIMAL(10,2),
        @pagado_alojamiento DECIMAL(10,2),
        @saldo_alojamiento DECIMAL(10,2),
        @estado_reserva VARCHAR(20);

    SELECT
        @total_alojamiento = total,
        @estado_reserva = estado
    FROM Reservas
    WHERE id_reserva = @id_reserva;

    IF @total_alojamiento IS NULL
        THROW 50026, ''La reserva no existe.'', 1;

    IF @estado_reserva <> ''CONFIRMADA''
        THROW 50027, ''La reserva debe estar CONFIRMADA para realizar el check-in.'', 1;

    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago IN (''PAGO_INICIAL'', ''SALDO_ALOJAMIENTO'');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    IF @saldo_alojamiento > 0
        THROW 50028, ''No se puede realizar el check-in porque existe saldo de alojamiento pendiente.'', 1;

    UPDATE Reservas
    SET
        estado = ''CHECKIN'',
        fecha_checkin = GETDATE()
    WHERE id_reserva = @id_reserva;

    SELECT
        id_reserva,
        estado,
        fecha_checkin
    FROM Reservas
    WHERE id_reserva = @id_reserva;
END;', @nuevo0 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckIn
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
IF @actual0 IS NULL OR (LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@anterior0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 AND LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@nuevo0,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2)
 THROW 51705, 'Definicion previa desconocida: revisar sin sobrescribir.', 1;
SELECT N'dbo.sp_Reserva_CheckIn' AS procedimiento, @actual0 AS definicion_previa;
DECLARE @actual1 nvarchar(max)=OBJECT_DEFINITION(OBJECT_ID(N'dbo.sp_Reserva_Cancelar')), @anterior1 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Cancelar
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @estado VARCHAR(20);

    SELECT @estado = estado
    FROM dbo.Reservas
    WHERE id_reserva = @id_reserva;

    IF @estado IS NULL
        THROW 50040, ''La reserva no existe.'', 1;

    IF @estado = ''CANCELADA''
        THROW 50041, ''La reserva ya esta cancelada.'', 1;

    IF @estado <> ''CONFIRMADA''
        THROW 50042, ''Solo se pueden cancelar reservas confirmadas.'', 1;

    UPDATE dbo.Reservas
    SET estado = ''CANCELADA''
    WHERE id_reserva = @id_reserva;
END;', @nuevo1 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Cancelar
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
IF @actual1 IS NULL OR (LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@anterior1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 AND LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@actual1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2 <> LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@nuevo1,CHAR(13),N''),N'CREATE OR ALTER PROCEDURE',N'PROCEDURE'),N'CREATE PROCEDURE',N'PROCEDURE'),N'ALTER PROCEDURE',N'PROCEDURE'),N'PROCEDURE dbo.',N'PROCEDURE '))) COLLATE Latin1_General_100_BIN2)
 THROW 51705, 'Definicion previa desconocida: revisar sin sobrescribir.', 1;
SELECT N'dbo.sp_Reserva_Cancelar' AS procedimiento, @actual1 AS definicion_previa;
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckIn
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
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Cancelar
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
 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
