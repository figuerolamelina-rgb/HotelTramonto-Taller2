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
DECLARE @nombre sysname, @version int, @texto nvarchar(max), @canon nvarchar(max),
        @revisado nvarchar(max), @mensaje nvarchar(2048);
DECLARE contratos CURSOR LOCAL FAST_FORWARD FOR
    SELECT nombre, version, texto FROM (VALUES
        (N'dbo.sp_Reserva_CheckIn', 2, @nuevo0),
        (N'dbo.sp_Reserva_CheckIn', 3, @actual0),
        (N'dbo.sp_Reserva_Cancelar', 2, @nuevo1),
        (N'dbo.sp_Reserva_Cancelar', 3, @actual1)
    ) AS candidatos(nombre, version, texto) ORDER BY nombre, version;
OPEN contratos;
FETCH NEXT FROM contratos INTO @nombre,@version,@texto;
WHILE @@FETCH_STATUS=0
BEGIN
    -- INICIO canon de ciclo (entrada @texto, salida @canon)
    DECLARE @pos int, @fin int, @ddl nvarchar(max), @enLiteral bit, @car nchar(1);
    SET @canon = NULL;
    IF @texto IS NOT NULL
    BEGIN
        -- Sólo se omiten espacios y comentarios de línea antes de la cabecera.
        WHILE 1=1
        BEGIN
            WHILE UNICODE(LEFT(@texto,1)) IN (9,10,13,32)
                SET @texto = STUFF(@texto,1,1,N'');
            IF COALESCE(LEFT(@texto,2),N'') COLLATE Latin1_General_100_BIN2 <> N'--' BREAK;
            SET @pos = PATINDEX(N'%['+NCHAR(13)+NCHAR(10)+N']%', @texto COLLATE Latin1_General_100_BIN2);
            IF @pos=0 BREAK;
            SET @texto = SUBSTRING(@texto COLLATE Latin1_General_100_BIN2,@pos+1,DATALENGTH(@texto)/2);
        END;
        SET @pos = CHARINDEX(N'PROCEDURE', @texto COLLATE Latin1_General_100_CI_AS);
        IF @pos>1 AND UNICODE(SUBSTRING(@texto,@pos-1,1)) IN (9,10,13,32)
            AND UNICODE(SUBSTRING(@texto,@pos+9,1)) IN (9,10,13,32)
        BEGIN
            SET @ddl = RTRIM(TRANSLATE(LEFT(@texto,@pos-1),NCHAR(9)+NCHAR(10)+NCHAR(13),N'   '));
            WHILE CHARINDEX(N'  ',@ddl)>0 SET @ddl = REPLACE(@ddl,N'  ',N' ');
            IF UPPER(@ddl COLLATE Latin1_General_100_BIN2) IN (N'CREATE',N'ALTER',N'CREATE OR ALTER')
            BEGIN
                SET @pos += 9;
                WHILE UNICODE(SUBSTRING(@texto,@pos,1)) IN (9,10,13,32) SET @pos += 1;
                IF SUBSTRING(@texto,@pos,4) COLLATE Latin1_General_100_BIN2 = N'dbo.' SET @pos += 4;
                -- Desde el nombre: comparación ordinal. Un fin de línea dentro de
                -- una cadena se copia tal cual, incluso si contiene comillas escapadas.
                SET @canon = N'';
                SET @enLiteral = 0;
                SET @fin = DATALENGTH(@texto)/2;
                WHILE @fin>=@pos AND UNICODE(SUBSTRING(@texto COLLATE Latin1_General_100_BIN2,@fin,1)) IN (9,10,13,32)
                    SET @fin -= 1;
                WHILE @pos<=@fin
                BEGIN
                    SET @car = SUBSTRING(@texto COLLATE Latin1_General_100_BIN2,@pos,1);
                    IF UNICODE(@car)=39 SET @enLiteral = 1-@enLiteral;
                    IF @enLiteral=0 AND UNICODE(@car)=13
                    BEGIN
                        IF UNICODE(SUBSTRING(@texto COLLATE Latin1_General_100_BIN2,@pos+1,1))=10 SET @pos += 1;
                        SET @car = NCHAR(10);
                    END;
                    SET @canon += @car;
                    SET @pos += 1;
                END;
            END;
        END;
    END;
    -- FIN canon de ciclo
    IF @version=2 SET @revisado=@canon;
    IF @version<3 AND @canon IS NULL
    BEGIN
        SET @mensaje = CONCAT(@nombre, N': Cabecera del contrato versionado desconocida.');
        THROW 51706, @mensaje, 1;
    END;
    IF @version=3 AND (@canon IS NULL OR NOT (
        (DATALENGTH(@canon)=DATALENGTH(@revisado) AND @canon COLLATE Latin1_General_100_BIN2 = @revisado)))
    BEGIN
        SET @mensaje = CONCAT(@nombre, N': Definicion de ciclo distinta de la revisada.');
        THROW 51706, @mensaje, 1;
    END;
    FETCH NEXT FROM contratos INTO @nombre,@version,@texto;
END;
CLOSE contratos;
DEALLOCATE contratos;
SELECT N'Definiciones de check-in y cancelacion verificadas; concurrencia no ejecutada por esta consulta' AS resultado;
