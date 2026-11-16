IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
    THROW 51300, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51301, 'Ejecutar sin transaccion externa.', 1;
IF COL_LENGTH('dbo.Pagos','id_operacion') IS NOT NULL
 THROW 51434, 'Concurrencia instalada: usar 06_SuspenderEscrituras.sql y revisar respaldo. No restaurar caminos inseguros.', 1;
-- Ejecutar solo tras detener TODOS los clientes y respaldar la base.
-- Conserva columnas, FK, indice, consumos, precios y pagos. Suspende nuevos consumos.
-- Restaura contratos previos de consulta/pago/check-out de la rama base.
-- Se pierde la coordinacion nueva: no reabrir operaciones hasta resolver la incidencia.
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN (OBJECT_ID('dbo.sp_DetalleServicio_Insertar'),OBJECT_ID('dbo.sp_DetalleServicio_ListarPorReserva'),OBJECT_ID('dbo.sp_Pago_RegistrarServicios'),OBJECT_ID('dbo.sp_Reserva_CheckOut'))) THROW 51305, 'Modulos firmados: coordinar con DBA.', 1;
BEGIN TRY
 BEGIN TRANSACTION;
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_DetalleServicio_Insertar
 @id_servicios INT, @id_reserva INT, @cantidad INT, @precio_unitario DECIMAL(10,2),
 @id_habitacion INT=NULL, @id_operacion UNIQUEIDENTIFIER=NULL
AS
 THROW 51330, ''Registro de consumos suspendido por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_DetalleServicio_ListarPorReserva
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DS.id_detalle_servicio,
        DS.id_reserva,
        DS.id_servicios,
        S.nombre AS nombre_servicio,
        DS.cantidad,
        DS.precio_unitario,
        (DS.cantidad * DS.precio_unitario) AS subtotal
    FROM DetalleServicios DS
    INNER JOIN Servicios S
        ON DS.id_servicios = S.id_servicio
    WHERE DS.id_reserva = @id_reserva
    ORDER BY S.nombre;
END;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarServicios
    @id_reserva INT,
    @id_usuario INT,
    @metodo_pago VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @total_servicios DECIMAL(10,2),
        @pagado_servicios DECIMAL(10,2),
        @saldo_servicios DECIMAL(10,2);

    -- calcular total consumido en servicios
    SELECT
        @total_servicios =
            ISNULL(SUM(cantidad * precio_unitario), 0)
    FROM DetalleServicios
    WHERE id_reserva = @id_reserva;

    IF @total_servicios <= 0
        THROW 50024, ''La reserva no tiene servicios adicionales para pagar.'', 1;

    -- calcular lo ya pagado de servicios
    SELECT
        @pagado_servicios =
            ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago = ''SERVICIOS_ADICIONALES'';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_servicios <= 0
        THROW 50025, ''Los servicios adicionales ya están pagados.'', 1;

    -- registrar exactamente el saldo pendiente
    INSERT INTO Pagos
    (
        id_reserva,
        id_reserva_temporal,
        id_usuario,
        monto,
        metodo_pago,
        estado,
        tipo_pago
    )
    VALUES
    (
        @id_reserva,
        NULL,
        @id_usuario,
        @saldo_servicios,
        @metodo_pago,
        ''APROBADO'',
        ''SERVICIOS_ADICIONALES''
    );

    SELECT
        SCOPE_IDENTITY() AS id_pago,
        @id_reserva AS id_reserva,
        @saldo_servicios AS monto_pagado,
        ''SERVICIOS_ADICIONALES'' AS tipo_pago;
END;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckOut
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @estado_reserva VARCHAR(20),
        @total_alojamiento DECIMAL(10,2),
        @pagado_alojamiento DECIMAL(10,2),
        @saldo_alojamiento DECIMAL(10,2),
        @total_servicios DECIMAL(10,2),
        @pagado_servicios DECIMAL(10,2),
        @saldo_servicios DECIMAL(10,2);

    SELECT
        @estado_reserva = estado,
        @total_alojamiento = total
    FROM Reservas
    WHERE id_reserva = @id_reserva;

    IF @estado_reserva IS NULL
        THROW 50029, ''La reserva no existe.'', 1;

    IF @estado_reserva <> ''CHECKIN''
        THROW 50030, ''La reserva debe estar en CHECKIN para realizar el check-out.'', 1;

    -- saldo de alojamiento
    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago IN (''PAGO_INICIAL'', ''SALDO_ALOJAMIENTO'');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    -- saldo de servicios
    SELECT
        @total_servicios = ISNULL(SUM(cantidad * precio_unitario), 0)
    FROM DetalleServicios
    WHERE id_reserva = @id_reserva;

    SELECT
        @pagado_servicios = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago = ''SERVICIOS_ADICIONALES'';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_alojamiento > 0
        THROW 50031, ''No se puede realizar el check-out porque existe saldo de alojamiento pendiente.'', 1;

    IF @saldo_servicios > 0
        THROW 50032, ''No se puede realizar el check-out porque existen servicios pendientes de pago.'', 1;

    UPDATE Reservas
    SET
        estado = ''FINALIZADA'',
        fecha_checkout = GETDATE()
    WHERE id_reserva = @id_reserva;

    SELECT
        id_reserva,
        estado,
        fecha_checkout
    FROM Reservas
    WHERE id_reserva = @id_reserva;
END;';
 COMMIT;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK;
 THROW;
END CATCH;
