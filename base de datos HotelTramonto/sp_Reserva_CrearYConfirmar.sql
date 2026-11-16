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

        IF @fecha_salida <= @fecha_entrada
            THROW 50051, 'La fecha de salida debe ser posterior a la entrada.', 1;

        IF @modalidad_pago NOT IN ('PAGO_TOTAL', 'SENA_50')
            THROW 50052, 'La modalidad de pago no es válida.', 1;

        IF @monto <= 0
            THROW 50053, 'El monto debe ser mayor que cero.', 1;

        IF @cantidad_personas <= 0
            THROW 50054, 'La cantidad de personas debe ser mayor que cero.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM dbo.Usuarios
            WHERE id_usuario = @id_usuario
              AND estado = 'ACTIVO'
        )
            THROW 50055, 'El usuario no existe o no está activo.', 1;

        DECLARE @id_temporal INT;

        INSERT INTO dbo.ReservasTemporales
        (
            id_huesped,
            id_habitacion,
            estado,
            fecha_entrada,
            fecha_salida,
            fecha_expiracion,
            modalidad_pago
        )
        VALUES
        (
            @id_huesped,
            @id_habitacion,
            'PENDIENTE',
            @fecha_entrada,
            @fecha_salida,
            DATEADD(MINUTE, 30, GETDATE()),
            @modalidad_pago
        );

        SET @id_temporal = SCOPE_IDENTITY();

        INSERT INTO dbo.Pagos
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
            NULL,
            @id_temporal,
            @id_usuario,
            @monto,
            @metodo_pago,
            'APROBADO',
            'PAGO_INICIAL'
        );

        EXEC dbo.sp_ConfirmarReserva
            @id_reserva_temporal = @id_temporal,
            @cantidad_personas = @cantidad_personas;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO