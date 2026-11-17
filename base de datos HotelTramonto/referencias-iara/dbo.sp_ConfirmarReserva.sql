USE HotelTramontoDB;
GO

ALTER PROCEDURE dbo.sp_ConfirmarReserva
    @id_reserva_temporal INT,
    @cantidad_personas INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE
            @id_huesped INT,
            @id_habitacion INT,
            @estado_temporal VARCHAR(20),
            @modalidad_pago VARCHAR(20),
            @fecha_entrada DATE,
            @fecha_salida DATE,
            @fecha_expiracion DATETIME,
            @id_reserva_existente INT,
            @id_usuario INT,
            @id_pago INT,
            @monto_pago DECIMAL(10,2),
            @precio_noche DECIMAL(10,2),
            @capacidad INT,
            @cantidad_noches INT,
            @total_alojamiento DECIMAL(10,2),
            @mitad_alojamiento DECIMAL(10,2),
            @id_reserva_nueva INT;

        -- 1. Bloquear y obtener la reserva temporal.
        SELECT
            @id_huesped = RT.id_huesped,
            @id_habitacion = RT.id_habitacion,
            @estado_temporal = RT.estado,
            @modalidad_pago = RT.modalidad_pago,
            @fecha_entrada = RT.fecha_entrada,
            @fecha_salida = RT.fecha_salida,
            @fecha_expiracion = RT.fecha_expiracion,
            @id_reserva_existente = RT.id_reserva
        FROM dbo.ReservasTemporales RT WITH (UPDLOCK, HOLDLOCK)
        WHERE RT.id_reserva_temporal = @id_reserva_temporal;

        IF @id_huesped IS NULL
            THROW 50001, 'La reserva temporal no existe.', 1;

        IF @id_reserva_existente IS NOT NULL
            THROW 50002, 'La reserva temporal ya fue confirmada.', 1;

        IF @estado_temporal <> 'PENDIENTE'
            THROW 50012,
                'Solo se pueden confirmar reservas temporales pendientes.', 1;

        IF @fecha_expiracion <= GETDATE()
            THROW 50003, 'La reserva temporal se encuentra expirada.', 1;

        -- 2. Bloquear habitación y obtener precio y capacidad.
        SELECT
            @precio_noche = TH.precio_noche,
            @capacidad = TH.capacidad
        FROM dbo.Habitaciones H WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.TiposHabitacion TH
            ON H.tipo_habitacion = TH.id_tipoHabitacion
        WHERE H.id_habitacion = @id_habitacion
          AND H.activo = 1
          AND TH.activo = 1
          AND H.estado <> 'MANTENIMIENTO';

        IF @precio_noche IS NULL
            THROW 50005,
                'La habitación no está disponible para reservas.', 1;

        IF @cantidad_personas <= 0
           OR @cantidad_personas > @capacidad
            THROW 50006,
                'La cantidad de personas supera la capacidad de la habitación.', 1;

        -- 3. Verificar reservas definitivas superpuestas.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.DetalleReservas DR
            INNER JOIN dbo.Reservas R
                ON DR.id_reserva = R.id_reserva
            WHERE DR.id_habitacion = @id_habitacion
              AND R.estado IN ('CONFIRMADA', 'CHECKIN')
              AND R.fecha_entrada < @fecha_salida
              AND R.fecha_salida > @fecha_entrada
        )
            THROW 50007,
                'La habitación ya tiene una reserva definitiva para esas fechas.', 1;

        -- 4. NUEVO: verificar otras reservas temporales vigentes.
        IF EXISTS
        (
            SELECT 1
            FROM dbo.ReservasTemporales RT WITH (UPDLOCK, HOLDLOCK)
            WHERE RT.id_habitacion = @id_habitacion
              AND RT.id_reserva_temporal <> @id_reserva_temporal
              AND RT.estado = 'PENDIENTE'
              AND RT.id_reserva IS NULL
              AND RT.fecha_expiracion > GETDATE()
              AND RT.fecha_entrada < @fecha_salida
              AND RT.fecha_salida > @fecha_entrada
        )
            THROW 50013,
                'Existe otra reserva temporal vigente para esa habitación y esas fechas.', 1;

        -- 5. Buscar el pago inicial aprobado.
        SELECT TOP 1
            @id_pago = P.id_pago,
            @id_usuario = P.id_usuario,
            @monto_pago = P.monto
        FROM dbo.Pagos P WITH (UPDLOCK, HOLDLOCK)
        WHERE P.id_reserva_temporal = @id_reserva_temporal
          AND P.id_reserva IS NULL
          AND P.tipo_pago = 'PAGO_INICIAL'
          AND P.estado = 'APROBADO'
        ORDER BY P.fecha_pago DESC, P.id_pago DESC;

        IF @id_pago IS NULL
            THROW 50008,
                'No existe un pago inicial aprobado para esta reserva temporal.', 1;

        IF @id_usuario IS NULL
           OR NOT EXISTS
           (
               SELECT 1
               FROM dbo.Usuarios
               WHERE id_usuario = @id_usuario
                 AND activo = 1
           )
            THROW 50041,
                'La reserva debe tener un usuario responsable activo.', 1;

        -- 6. Calcular alojamiento.
        SET @cantidad_noches =
            DATEDIFF(DAY, @fecha_entrada, @fecha_salida);

        SET @total_alojamiento =
            @cantidad_noches * @precio_noche;

        SET @mitad_alojamiento =
            ROUND(@total_alojamiento * 0.50, 2);

        IF @modalidad_pago IS NULL
           OR @modalidad_pago NOT IN ('PAGO_TOTAL', 'SENA_50')
            THROW 50009,
                'La modalidad de pago no es válida.', 1;

        IF @modalidad_pago = 'PAGO_TOTAL'
           AND @monto_pago <> @total_alojamiento
            THROW 50010,
                'PAGO_TOTAL requiere abonar el 100% del alojamiento.', 1;

        IF @modalidad_pago = 'SENA_50'
           AND @monto_pago <> @mitad_alojamiento
            THROW 50011,
                'SENA_50 requiere abonar el 50% del alojamiento.', 1;

        -- 7. Crear reserva definitiva.
        INSERT INTO dbo.Reservas
        (
            id_huesped,
            id_usuario,
            fecha_entrada,
            fecha_salida,
            estado,
            total,
            modalidad_pago,
            observaciones
        )
        VALUES
        (
            @id_huesped,
            @id_usuario,
            @fecha_entrada,
            @fecha_salida,
            'CONFIRMADA',
            @total_alojamiento,
            @modalidad_pago,
            'Reserva confirmada desde reserva temporal'
        );

        SET @id_reserva_nueva = CONVERT(INT, SCOPE_IDENTITY());

        -- 8. Registrar habitación.
        INSERT INTO dbo.DetalleReservas
        (
            id_reserva,
            id_habitacion,
            precio_noche,
            cantidad_personas
        )
        VALUES
        (
            @id_reserva_nueva,
            @id_habitacion,
            @precio_noche,
            @cantidad_personas
        );

        -- 9. Asociar pago a reserva definitiva.
        UPDATE dbo.Pagos
        SET
            id_reserva = @id_reserva_nueva,
            id_reserva_temporal = NULL
        WHERE id_pago = @id_pago;

        -- 10. Confirmar reserva temporal.
        UPDATE dbo.ReservasTemporales
        SET
            id_reserva = @id_reserva_nueva,
            estado = 'CONFIRMADA'
        WHERE id_reserva_temporal = @id_reserva_temporal;

        COMMIT TRANSACTION;

        SELECT
            @id_reserva_nueva AS id_reserva,
            @total_alojamiento AS total_alojamiento,
            @monto_pago AS pago_inicial,
            (@total_alojamiento - @monto_pago) AS saldo_alojamiento;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO