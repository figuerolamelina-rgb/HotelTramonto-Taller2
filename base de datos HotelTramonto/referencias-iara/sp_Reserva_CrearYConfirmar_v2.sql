USE HotelTramontoDB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CrearYConfirmar
    @id_huesped INT,
    @id_habitacion INT,
    @fecha_entrada DATE,
    @fecha_salida DATE,
    @modalidad_pago VARCHAR(20),
    @id_usuario INT,
    @monto DECIMAL(10,2),
    @metodo_pago VARCHAR(30),
    @cantidad_personas INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        BEGIN TRANSACTION;

        -- Validaciones iniciales
        IF @id_huesped <= 0 OR @id_habitacion <= 0
            THROW 50101, 'Huésped o habitación inválidos.', 1;

        IF @id_usuario <= 0
            THROW 50102, 'Usuario inválido.', 1;

        IF @fecha_entrada < CAST(GETDATE() AS DATE)
           OR @fecha_salida <= @fecha_entrada
            THROW 50103, 'Las fechas de la reserva no son válidas.', 1;

        IF @cantidad_personas <= 0
            THROW 50104, 'La cantidad de personas debe ser mayor que cero.', 1;

        IF @modalidad_pago NOT IN ('PAGO_TOTAL', 'SENA_50')
            THROW 50105, 'Modalidad de pago inválida.', 1;

        IF @monto <= 0
            THROW 50106, 'El importe del pago debe ser mayor que cero.', 1;

        -- Crear reserva temporal y obtener su ID
        DECLARE @Temporal TABLE
        (
            id_reserva_temporal INT
        );

        INSERT INTO @Temporal
        EXEC dbo.sp_ReservaTemporal_Insertar
            @id_huesped = @id_huesped,
            @id_habitacion = @id_habitacion,
            @fecha_entrada = @fecha_entrada,
            @fecha_salida = @fecha_salida,
            @modalidad_pago = @modalidad_pago;

        DECLARE @id_temporal INT;

        SELECT @id_temporal = id_reserva_temporal
        FROM @Temporal;

        IF @id_temporal IS NULL
            THROW 50107, 'No se pudo crear la reserva temporal.', 1;

        -- Registrar pago inicial
        EXEC dbo.sp_Pago_Insertar
            @id_reserva = NULL,
            @id_reserva_temporal = @id_temporal,
            @id_usuario = @id_usuario,
            @monto = @monto,
            @metodo_pago = @metodo_pago,
            @estado = 'APROBADO',
            @tipo_pago = 'PAGO_INICIAL';

        -- Confirmar reserva definitiva
        DECLARE @Resultado TABLE
        (
            id_reserva INT,
            total_alojamiento DECIMAL(10,2),
            pago_inicial DECIMAL(10,2),
            saldo_alojamiento DECIMAL(10,2)
        );

        INSERT INTO @Resultado
        EXEC dbo.sp_ConfirmarReserva
            @id_reserva_temporal = @id_temporal,
            @cantidad_personas = @cantidad_personas;

        IF NOT EXISTS (SELECT 1 FROM @Resultado)
            THROW 50108, 'No se obtuvo la confirmación de la reserva.', 1;

        COMMIT TRANSACTION;

        -- Devolver información a C#
        SELECT
            @id_temporal AS id_reserva_temporal,
            id_reserva,
            total_alojamiento,
            pago_inicial,
            saldo_alojamiento
        FROM @Resultado;

    END TRY
    BEGIN CATCH

        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;

    END CATCH;
END;
GO