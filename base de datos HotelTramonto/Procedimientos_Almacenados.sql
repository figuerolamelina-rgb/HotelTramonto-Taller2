USE HotelTramontoDB;
GO

CREATE OR ALTER PROCEDURE sp_Huesped_Insertar
    @nombre VARCHAR(50),
    @apellido VARCHAR(50),
    @dni VARCHAR(15),
    @telefono VARCHAR(20) = NULL,
    @email VARCHAR(100) = NULL,
    @nacionalidad VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Huespedes
        (nombre, apellido, dni, telefono, email, nacionalidad)
    VALUES
        (@nombre, @apellido, @dni, @telefono, @email, @nacionalidad);
END;
GO




CREATE OR ALTER PROCEDURE sp_Huesped_Modificar  --modificamos la info del huesped
    @id_huesped INT,
    @nombre VARCHAR(50),
    @apellido VARCHAR(50),
    @dni VARCHAR(15),
    @telefono VARCHAR(20) = NULL,
    @email VARCHAR(100) = NULL,
    @nacionalidad VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Huespedes
    SET
        nombre = @nombre,
        apellido = @apellido,
        dni = @dni,
        telefono = @telefono,
        email = @email,
        nacionalidad = @nacionalidad
    WHERE id_huesped = @id_huesped;
END;
GO





CREATE OR ALTER PROCEDURE sp_Huesped_Desactivar
    @id_huesped INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Huespedes
    SET activo = 0
    WHERE id_huesped = @id_huesped;
END;
GO






CREATE OR ALTER PROCEDURE dbo.sp_Huesped_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_huesped,
        nombre,
        apellido,
        dni,
        telefono,
        email,
        nacionalidad,
        activo,
        fecha_alta
    FROM Huespedes
    ORDER BY activo DESC, apellido, nombre;
END;
GO



CREATE OR ALTER PROCEDURE sp_Huesped_BuscarPorDNI --buscamos huesped por dni
    @dni VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_huesped,
        nombre,
        apellido,
        dni,
        telefono,
        email,
        nacionalidad,
        activo,
        fecha_alta
    FROM Huespedes
    WHERE dni = @dni;
END;
GO



--insertamos usuario


CREATE OR ALTER PROCEDURE sp_Usuario_Insertar
    @id_rol INT,
    @nombre VARCHAR(50),
    @apellido VARCHAR(50),
    @dni VARCHAR(15),
    @email VARCHAR(100),
    @contrasena VARCHAR(255),
    @telefono VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validar que el email sea obligatorio
    IF @email IS NULL OR LTRIM(RTRIM(@email)) = ''
    BEGIN
        THROW 50032, 'El email del usuario es obligatorio.', 1;
    END;

    -- Validar que el email no esté repetido
    IF EXISTS (
        SELECT 1
        FROM dbo.Usuarios
        WHERE email = @email
    )
    BEGIN
        THROW 50033, 'Ya existe un usuario con ese email.', 1;
    END;

    INSERT INTO dbo.Usuarios
        (id_rol, nombre, apellido, dni, email, contrasena, telefono)
    VALUES
        (@id_rol, @nombre, @apellido, @dni, @email, @contrasena, @telefono);
END;
GO





--modificar usuario


CREATE OR ALTER PROCEDURE sp_Usuario_Modificar
    @id_usuario INT,
    @id_rol INT,
    @nombre VARCHAR(50),
    @apellido VARCHAR(50),
    @dni VARCHAR(15),
    @email VARCHAR(100),
    @telefono VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- El email es obligatorio
    IF @email IS NULL OR LTRIM(RTRIM(@email)) = ''
    BEGIN
        THROW 50032, 'El email del usuario es obligatorio.', 1;
    END;

    -- No permitir emails de otros usuarios
    IF EXISTS (
        SELECT 1
        FROM dbo.Usuarios
        WHERE email = @email
          AND id_usuario <> @id_usuario
    )
    BEGIN
        THROW 50033, 'Ya existe otro usuario con ese email.', 1;
    END;

    UPDATE dbo.Usuarios
    SET
        id_rol = @id_rol,
        nombre = @nombre,
        apellido = @apellido,
        dni = @dni,
        email = @email,
        telefono = @telefono
    WHERE id_usuario = @id_usuario;
END;
GO


--baja lógica de Usuario

CREATE OR ALTER PROCEDURE sp_Usuario_Desactivar
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuarios
    SET activo = 0
    WHERE id_usuario = @id_usuario;
END;
GO



--listar usuarios

CREATE OR ALTER PROCEDURE sp_Usuario_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.id_usuario,
        U.id_rol,
        R.nombre_rol,
        U.nombre,
        U.apellido,
        U.dni,
        U.email,
        U.telefono,
        U.activo,
        U.fecha_alta
    FROM Usuarios U
    INNER JOIN Roles R
        ON U.id_rol = R.id_rol
    ORDER BY U.apellido, U.nombre;
END;
GO


--buscar usuario por DNI.

CREATE OR ALTER PROCEDURE sp_Usuario_BuscarPorDNI
    @dni VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.id_usuario,
        U.id_rol,
        R.nombre_rol,
        U.nombre,
        U.apellido,
        U.dni,
        U.email,
        U.telefono,
        U.activo,
        U.fecha_alta
    FROM Usuarios U
    INNER JOIN Roles R
        ON U.id_rol = R.id_rol
    WHERE U.dni = @dni;
END;
GO




--contraseña de usuario

CREATE OR ALTER PROCEDURE sp_Usuario_CambiarContrasena
    @id_usuario INT,
    @contrasena VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuarios
    SET contrasena = @contrasena
    WHERE id_usuario = @id_usuario;
END;
GO




--TiposHabitacion

CREATE OR ALTER PROCEDURE sp_TipoHabitacion_Insertar
    @nombre VARCHAR(50),
    @descripcion VARCHAR(200) = NULL,
    @precio_noche DECIMAL(10,2),
    @capacidad INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TiposHabitacion
        (nombre, descripcion, precio_noche, capacidad)
    VALUES
        (@nombre, @descripcion, @precio_noche, @capacidad);
END;
GO


--modificar tipo de habitación

CREATE OR ALTER PROCEDURE sp_TipoHabitacion_Modificar
    @id_tipoHabitacion INT,
    @nombre VARCHAR(50),
    @descripcion VARCHAR(200) = NULL,
    @precio_noche DECIMAL(10,2),
    @capacidad INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TiposHabitacion
    SET
        nombre = @nombre,
        descripcion = @descripcion,
        precio_noche = @precio_noche,
        capacidad = @capacidad
    WHERE id_tipoHabitacion = @id_tipoHabitacion;
END;
GO


-- desactivar tipo de habitación.

CREATE OR ALTER PROCEDURE sp_TipoHabitacion_Desactivar
    @id_tipoHabitacion INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TiposHabitacion
    SET activo = 0
    WHERE id_tipoHabitacion = @id_tipoHabitacion;
END;
GO




-- listar tipos de habitación activos.

CREATE OR ALTER PROCEDURE sp_TipoHabitacion_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_tipoHabitacion,
        nombre,
        descripcion,
        precio_noche,
        capacidad,
        activo
    FROM TiposHabitacion
    WHERE activo = 1
    ORDER BY nombre;
END;
GO




--Habitaciones.
--insertar una habitación

CREATE OR ALTER PROCEDURE sp_Habitacion_Insertar
    @tipo_habitacion INT,
    @piso INT = NULL,
    @numero VARCHAR(10),
    @estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Habitaciones
        (tipo_habitacion, piso, numero, estado)
    VALUES
        (@tipo_habitacion, @piso, @numero, @estado);
END;
GO


--modificar habitación.
CREATE OR ALTER PROCEDURE sp_Habitacion_Modificar
    @id_habitacion INT,
    @tipo_habitacion INT,
    @piso INT = NULL,
    @numero VARCHAR(10),
    @estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Habitaciones
    SET
        tipo_habitacion = @tipo_habitacion,
        piso = @piso,
        numero = @numero,
        estado = @estado
    WHERE id_habitacion = @id_habitacion;
END;
GO


--desactivar habitación.


CREATE OR ALTER PROCEDURE sp_Habitacion_Desactivar
    @id_habitacion INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Habitaciones
    SET activo = 0
    WHERE id_habitacion = @id_habitacion;
END;
GO




--listar habitaciones activas.


CREATE OR ALTER PROCEDURE sp_Habitacion_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        H.id_habitacion,
        H.tipo_habitacion,
        T.nombre AS tipo,
        H.piso,
        H.numero,
        H.estado,
        H.activo
    FROM Habitaciones H
    INNER JOIN TiposHabitacion T
        ON H.tipo_habitacion = T.id_tipoHabitacion
    ORDER BY H.numero;
END;
GO




---Servicios.

CREATE OR ALTER PROCEDURE sp_Servicio_Insertar
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Servicios
        (nombre, descripcion, precio)
    VALUES
        (@nombre, @descripcion, @precio);
END;
GO



---modificar servicio.

CREATE OR ALTER PROCEDURE sp_Servicio_Modificar
    @id_servicio INT,
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Servicios
    SET
        nombre = @nombre,
        descripcion = @descripcion,
        precio = @precio
    WHERE id_servicio = @id_servicio;
END;
GO


---desactivar servicio.


CREATE OR ALTER PROCEDURE sp_Servicio_Desactivar
    @id_servicio INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Servicios
    SET activo = 0
    WHERE id_servicio = @id_servicio;
END;
GO



---listar servicios activos.

CREATE OR ALTER PROCEDURE sp_Servicio_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_servicio,
        nombre,
        descripcion,
        precio,
        activo
    FROM Servicios
    WHERE activo = 1
    ORDER BY nombre;
END;
GO





--- reservas


CREATE OR ALTER PROCEDURE sp_Reserva_Insertar
    @id_huesped INT,
    @id_usuario INT = NULL,
    @fecha_entrada DATE,
    @fecha_salida DATE,
    @estado VARCHAR(20),
    @total DECIMAL(10,2),
    @observaciones VARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    THROW 50020,
        'La creacion directa de reservas esta deshabilitada. Utilice sp_ConfirmarReserva.',
        1;
END;
GO

--modificar reserva.


CREATE OR ALTER PROCEDURE sp_Reserva_Modificar
    @id_reserva INT,
    @id_huesped INT,
    @id_usuario INT = NULL,
    @fecha_entrada DATE,
    @fecha_salida DATE,
    @estado VARCHAR(20),
    @total DECIMAL(10,2),
    @observaciones VARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    THROW 50021,
        'La modificacion directa de reservas esta deshabilitada hasta implementar las validaciones correspondientes.',
        1;
END;
GO




--listar reservas

CREATE OR ALTER PROCEDURE sp_Reserva_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.id_reserva,
        R.id_huesped,
        H.nombre AS nombre_huesped,
        H.apellido AS apellido_huesped,
        R.id_usuario,
        U.nombre AS nombre_usuario,
        U.apellido AS apellido_usuario,
        R.fecha_entrada,
        R.fecha_salida,
        R.estado,
        R.total,
        R.observaciones,
        R.fecha_creacion,
        R.fecha_checkin,
        R.fecha_checkout
    FROM Reservas R
    INNER JOIN Huespedes H
        ON R.id_huesped = H.id_huesped
    LEFT JOIN Usuarios U
        ON R.id_usuario = U.id_usuario
    ORDER BY R.fecha_creacion DESC;
END;
GO




---DetalleReservas

CREATE OR ALTER PROCEDURE sp_DetalleReserva_Insertar
    @id_reserva INT,
    @id_habitacion INT,
    @precio_noche DECIMAL(10,2),
    @cantidad_personas INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DetalleReservas
        (
            id_reserva,
            id_habitacion,
            precio_noche,
            cantidad_personas
        )
    VALUES
        (
            @id_reserva,
            @id_habitacion,
            @precio_noche,
            @cantidad_personas
        );
END;
GO


---modificar DetalleReserva


CREATE OR ALTER PROCEDURE sp_DetalleReserva_Modificar
    @id_detalle_reserva INT,
    @id_habitacion INT,
    @precio_noche DECIMAL(10,2),
    @cantidad_personas INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DetalleReservas
    SET
        id_habitacion = @id_habitacion,
        precio_noche = @precio_noche,
        cantidad_personas = @cantidad_personas
    WHERE id_detalle_reserva = @id_detalle_reserva;
END;
GO

--listar las habitaciones de una reserva

CREATE OR ALTER PROCEDURE sp_DetalleReserva_ListarPorReserva
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DR.id_detalle_reserva,
        DR.id_reserva,
        DR.id_habitacion,
        H.numero AS numero_habitacion,
        TH.nombre AS tipo_habitacion,
        DR.precio_noche,
        DR.cantidad_personas
    FROM DetalleReservas DR
    INNER JOIN Habitaciones H
        ON DR.id_habitacion = H.id_habitacion
    INNER JOIN TiposHabitacion TH
        ON H.tipo_habitacion = TH.id_tipoHabitacion
    WHERE DR.id_reserva = @id_reserva
    ORDER BY H.numero;
END;
GO



---pagos
--registrar un pago


CREATE OR ALTER PROCEDURE sp_Pago_Insertar
    @id_reserva INT = NULL,
    @id_reserva_temporal INT = NULL,
    @id_usuario INT,
    @monto DECIMAL(10,2),
    @metodo_pago VARCHAR(30),
    @estado VARCHAR(20),
    @tipo_pago VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

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
        @id_reserva_temporal,
        @id_usuario,
        @monto,
        @metodo_pago,
        @estado,
        @tipo_pago
    );

    -- Devolver el ID del pago recién registrado
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS id_pago;
END;
GO




CREATE OR ALTER PROCEDURE sp_ConfirmarReserva
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


        -- 1. Obtener y bloquear la reserva temporal

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

        -- 2. Evitar confirmaciones duplicadas
        IF @id_reserva_existente IS NOT NULL
            THROW 50002, 'La reserva temporal ya fue confirmada.', 1;


        -- Verificar que la reserva temporal siga pendiente
        IF @estado_temporal IS NULL
           OR @estado_temporal <> 'PENDIENTE'
        BEGIN
            THROW 50012,
                'Solo se pueden confirmar reservas temporales pendientes.',
                1;
        END;

        -- 3. Verificar expiración
        IF @fecha_expiracion <= GETDATE()
            THROW 50003, 'La reserva temporal se encuentra expirada.', 1;



        -- 5. Obtener habitación, precio y capacidad
        SELECT
            @precio_noche = TH.precio_noche,
            @capacidad = TH.capacidad

     FROM Habitaciones H WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN TiposHabitacion TH
            ON H.tipo_habitacion = TH.id_tipoHabitacion
        WHERE H.id_habitacion = @id_habitacion
          AND H.activo = 1
          AND TH.activo = 1
          AND H.estado <> 'MANTENIMIENTO';

        IF @precio_noche IS NULL
            THROW 50005, 'La habitación no está disponible para reservas.', 1;

        IF @cantidad_personas <= 0 OR @cantidad_personas > @capacidad
            THROW 50006, 'La cantidad de personas supera la capacidad de la habitación.', 1;

        -- 6. Volver a verificar disponibilidad
        IF EXISTS
        (
            SELECT 1
            FROM DetalleReservas DR
            INNER JOIN Reservas R
                ON DR.id_reserva = R.id_reserva
            WHERE DR.id_habitacion = @id_habitacion
              AND R.estado IN ('CONFIRMADA', 'CHECKIN')
              AND R.fecha_entrada < @fecha_salida
              AND R.fecha_salida > @fecha_entrada
        )
            THROW 50007, 'La habitación ya no está disponible para esas fechas.', 1;

        -- 7. Buscar el pago inicial aprobado
        SELECT TOP 1
            @id_pago = P.id_pago,
            @id_usuario = P.id_usuario,
            @monto_pago = P.monto
        FROM Pagos P
        WHERE P.id_reserva_temporal = @id_reserva_temporal
          AND P.id_reserva IS NULL
          AND P.tipo_pago = 'PAGO_INICIAL'
          AND P.estado = 'APROBADO'
        ORDER BY P.fecha_pago DESC;

        IF @id_pago IS NULL
            THROW 50008, 'No existe un pago inicial aprobado para esta reserva temporal.', 1;

            -- Verificar que el usuario responsable exista y esté activo
            IF @id_usuario IS NULL
               OR NOT EXISTS (
                   SELECT 1
                   FROM dbo.Usuarios
                   WHERE id_usuario = @id_usuario
                     AND activo = 1
               )
            BEGIN
                THROW 50041,
                    'La reserva debe tener un usuario responsable activo.',
                    1;
            END;

        -- 8. Calcular importe del alojamiento
        SET @cantidad_noches = DATEDIFF(DAY, @fecha_entrada, @fecha_salida);
        SET @total_alojamiento = @cantidad_noches * @precio_noche;
        SET @mitad_alojamiento = ROUND(@total_alojamiento * 0.50, 2);

        -- Pago inicial permitido: 50% o 100%

        -- 8. Validar la modalidad de pago elegida

        IF @modalidad_pago IS NULL
           OR @modalidad_pago NOT IN ('PAGO_TOTAL', 'SENA_50')
        BEGIN
            THROW 50009,
                'La reserva temporal no tiene una modalidad de pago valida.',
                1;
        END;

        -- Si eligio pago total, debe abonar el 100%
        IF @modalidad_pago = 'PAGO_TOTAL'
           AND @monto_pago <> @total_alojamiento
        BEGIN
            THROW 50010,
                'La modalidad PAGO_TOTAL requiere pagar el 100% del alojamiento.',
                1;
        END;

        -- Si eligio sena, debe abonar el 50%
        IF @modalidad_pago = 'SENA_50'
           AND @monto_pago <> @mitad_alojamiento
        BEGIN
            THROW 50011,
                'La modalidad SENA_50 requiere pagar el 50% del alojamiento.',
                1;
        END;


        -- 9. Crear reserva definitiva
        -- Conservando la modalidad de pago elegida

        INSERT INTO Reservas
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


        SET @id_reserva_nueva = SCOPE_IDENTITY();

        -- 10. Guardar habitación y precio histórico
        INSERT INTO DetalleReservas
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

        -- 11. Vincular el pago existente con la reserva definitiva
        UPDATE Pagos
        SET
            id_reserva = @id_reserva_nueva,
            id_reserva_temporal = NULL
        WHERE id_pago = @id_pago;


        -- 12. Vincular temporal con definitiva
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


--modificar el estado de un pago.

CREATE OR ALTER PROCEDURE sp_Pago_ActualizarEstado
    @id_pago INT,
    @estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Pagos
    SET estado = @estado
    WHERE id_pago = @id_pago;
END;
GO


---listar los pagos de una reserva.

CREATE OR ALTER PROCEDURE sp_Pago_ListarPorReserva
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.id_pago,
        P.id_reserva,
        P.id_usuario,
        U.nombre AS nombre_usuario,
        U.apellido AS apellido_usuario,
        P.monto,
        P.metodo_pago,
        P.estado,
        P.tipo_pago,
        P.fecha_pago
    FROM Pagos P
    INNER JOIN Usuarios U
        ON P.id_usuario = U.id_usuario
    WHERE P.id_reserva = @id_reserva
    ORDER BY P.fecha_pago DESC;
END;
GO


---DetalleServicios
--agregar un servicio a una reserva

CREATE OR ALTER PROCEDURE sp_DetalleServicio_Insertar
    @id_servicios INT,
    @id_reserva INT,
    @cantidad INT,
    @precio_unitario DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO DetalleServicios
        (
            id_servicios,
            id_reserva,
            cantidad,
            precio_unitario
        )
    VALUES
        (
            @id_servicios,
            @id_reserva,
            @cantidad,
            @precio_unitario
        );
END;
GO


---listar servicios por reserva.

CREATE OR ALTER PROCEDURE sp_DetalleServicio_ListarPorReserva
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
END;
GO



--ReservasTemporales


CREATE OR ALTER PROCEDURE sp_ReservaTemporal_Insertar
    @id_huesped INT,
    @id_habitacion INT,
    @fecha_entrada DATE,
    @fecha_salida DATE,
    @modalidad_pago VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF @modalidad_pago NOT IN ('PAGO_TOTAL', 'SENA_50')
       OR @modalidad_pago IS NULL
    BEGIN
        THROW 50050, 'La modalidad de pago debe ser PAGO_TOTAL o SENA_50.', 1;
    END;

    INSERT INTO ReservasTemporales
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

    SELECT CAST(SCOPE_IDENTITY() AS INT)
        AS id_reserva_temporal;
END;
GO



---actualizar el estado de la reserva temporal.


CREATE OR ALTER PROCEDURE sp_ReservaTemporal_ActualizarEstado
    @id_reserva_temporal INT,
    @estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF @estado IS NULL
       OR @estado NOT IN (
           'PENDIENTE',
           'CONFIRMADA',
           'CANCELADA',
           'EXPIRADA'
       )
    BEGIN
        THROW 50051, 'Estado de reserva temporal no valido.', 1;
    END;

    UPDATE dbo.ReservasTemporales
    SET estado = @estado
    WHERE id_reserva_temporal = @id_reserva_temporal;
END;
GO

--marcar automáticamente como expiradas las reservas temporales vencidas.

CREATE OR ALTER PROCEDURE sp_ReservaTemporal_ExpirarVencidas
AS
BEGIN
    SET NOCOUNT ON;


UPDATE ReservasTemporales
SET estado = 'EXPIRADA'
WHERE estado = 'PENDIENTE'
  AND id_reserva IS NULL
  AND fecha_expiracion <= GETDATE();

END;
GO



--listar reservas temporales pendientes.

CREATE OR ALTER PROCEDURE sp_ReservaTemporal_ListarPendientes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RT.id_reserva_temporal,
        RT.id_huesped,
        H.nombre AS nombre_huesped,
        H.apellido AS apellido_huesped,
        RT.id_habitacion,
        HA.numero AS numero_habitacion,
        RT.estado,
        RT.fecha_entrada,
        RT.fecha_salida,
        RT.fecha_creacion,
        RT.fecha_expiracion
    FROM ReservasTemporales RT
    INNER JOIN Huespedes H
        ON RT.id_huesped = H.id_huesped
    INNER JOIN Habitaciones HA
        ON RT.id_habitacion = HA.id_habitacion

WHERE RT.estado = 'PENDIENTE'
  AND RT.fecha_expiracion > GETDATE()
  AND RT.id_reserva IS NULL
ORDER BY RT.fecha_creacion DESC;

END;
GO






---disponibilidad de habitaciones

CREATE OR ALTER PROCEDURE sp_Habitacion_ConsultarDisponibilidad
    @fecha_entrada DATE,
    @fecha_salida DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        H.id_habitacion,
        H.numero,
        H.piso,
        H.estado,
        TH.id_tipoHabitacion,
        TH.nombre AS tipo_habitacion,
        TH.precio_noche,
        TH.capacidad
    FROM Habitaciones H
    INNER JOIN TiposHabitacion TH
        ON H.tipo_habitacion = TH.id_tipoHabitacion
    WHERE H.activo = 1
      AND TH.activo = 1
      AND H.estado <> 'MANTENIMIENTO'
      AND NOT EXISTS
      (
          SELECT 1
          FROM DetalleReservas DR
          INNER JOIN Reservas R
              ON DR.id_reserva = R.id_reserva
          WHERE DR.id_habitacion = H.id_habitacion
            AND R.estado IN ('CONFIRMADA', 'CHECKIN')
            AND R.fecha_entrada < @fecha_salida
            AND R.fecha_salida > @fecha_entrada
      )
    ORDER BY H.numero;
END;
GO



---check-in

-- check-in: solo permite ingresar si el alojamiento está totalmente pagado

CREATE OR ALTER PROCEDURE sp_Reserva_CheckIn
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
        THROW 50026, 'La reserva no existe.', 1;

    IF @estado_reserva <> 'CONFIRMADA'
        THROW 50027, 'La reserva debe estar CONFIRMADA para realizar el check-in.', 1;

    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago IN ('PAGO_INICIAL', 'SALDO_ALOJAMIENTO');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    IF @saldo_alojamiento > 0
        THROW 50028, 'No se puede realizar el check-in porque existe saldo de alojamiento pendiente.', 1;

    UPDATE Reservas
    SET
        estado = 'CHECKIN',
        fecha_checkin = GETDATE()
    WHERE id_reserva = @id_reserva;

    SELECT
        id_reserva,
        estado,
        fecha_checkin
    FROM Reservas
    WHERE id_reserva = @id_reserva;
END;
GO




-- check-out: solo permite finalizar si no quedan saldos pendientes

CREATE OR ALTER PROCEDURE sp_Reserva_CheckOut
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
        THROW 50029, 'La reserva no existe.', 1;

    IF @estado_reserva <> 'CHECKIN'
        THROW 50030, 'La reserva debe estar en CHECKIN para realizar el check-out.', 1;

    -- saldo de alojamiento
    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago IN ('PAGO_INICIAL', 'SALDO_ALOJAMIENTO');

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
      AND estado = 'APROBADO'
      AND tipo_pago = 'SERVICIOS_ADICIONALES';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_alojamiento > 0
        THROW 50031, 'No se puede realizar el check-out porque existe saldo de alojamiento pendiente.', 1;

    IF @saldo_servicios > 0
        THROW 50032, 'No se puede realizar el check-out porque existen servicios pendientes de pago.', 1;

    UPDATE Reservas
    SET
        estado = 'FINALIZADA',
        fecha_checkout = GETDATE()
    WHERE id_reserva = @id_reserva;

    SELECT
        id_reserva,
        estado,
        fecha_checkout
    FROM Reservas
    WHERE id_reserva = @id_reserva;
END;
GO


--consultas de reservas por huésped.

CREATE OR ALTER PROCEDURE sp_Reserva_ListarPorHuesped
    @id_huesped INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.id_reserva,
        R.fecha_entrada,
        R.fecha_salida,
        R.estado,
        R.total,
        R.observaciones,
        R.fecha_creacion,
        R.fecha_checkin,
        R.fecha_checkout
    FROM Reservas R
    WHERE R.id_huesped = @id_huesped
    ORDER BY R.fecha_creacion DESC;
END;
GO


---listar reservas por rango de fechas.

CREATE OR ALTER PROCEDURE sp_Reserva_ListarPorFechas
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.id_reserva,
        R.id_huesped,
        H.nombre AS nombre_huesped,
        H.apellido AS apellido_huesped,
        R.fecha_entrada,
        R.fecha_salida,
        R.estado,
        R.total,
        R.fecha_creacion
    FROM Reservas R
    INNER JOIN Huespedes H
        ON R.id_huesped = H.id_huesped
    WHERE R.fecha_entrada <= @fecha_hasta
      AND R.fecha_salida >= @fecha_desde
    ORDER BY R.fecha_entrada;
END;
GO




--reservas por estado.

CREATE OR ALTER PROCEDURE sp_Reserva_ListarPorEstado
    @estado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.id_reserva,
        R.id_huesped,
        H.nombre AS nombre_huesped,
        H.apellido AS apellido_huesped,
        R.fecha_entrada,
        R.fecha_salida,
        R.estado,
        R.total,
        R.fecha_creacion
    FROM Reservas R
    INNER JOIN Huespedes H
        ON R.id_huesped = H.id_huesped
    WHERE R.estado = @estado
    ORDER BY R.fecha_entrada;
END;
GO

--- Reserva_ConsultarSaldo

-- consultar saldo de una reserva

CREATE OR ALTER PROCEDURE sp_Reserva_ConsultarSaldo
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @total_alojamiento DECIMAL(10,2),
        @pagado_alojamiento DECIMAL(10,2),
        @saldo_alojamiento DECIMAL(10,2),
        @total_servicios DECIMAL(10,2),
        @pagado_servicios DECIMAL(10,2),
        @saldo_servicios DECIMAL(10,2),
        @total_pendiente DECIMAL(10,2);

    -- total del alojamiento
    SELECT
        @total_alojamiento = total
    FROM Reservas
    WHERE id_reserva = @id_reserva;

    IF @total_alojamiento IS NULL
        THROW 50020, 'La reserva no existe.', 1;

    -- pagos aprobados correspondientes al alojamiento
    SELECT
        @pagado_alojamiento =
            ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago IN ('PAGO_INICIAL', 'SALDO_ALOJAMIENTO');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    -- total de servicios consumidos
    SELECT
        @total_servicios =
            ISNULL(SUM(cantidad * precio_unitario), 0)
    FROM DetalleServicios
    WHERE id_reserva = @id_reserva;

    -- pagos aprobados de servicios
    SELECT
        @pagado_servicios =
            ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago = 'SERVICIOS_ADICIONALES';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    SET @total_pendiente =
        @saldo_alojamiento + @saldo_servicios;

    SELECT
        @id_reserva AS id_reserva,
        @total_alojamiento AS total_alojamiento,
        @pagado_alojamiento AS pagado_alojamiento,
        @saldo_alojamiento AS saldo_alojamiento,
        @total_servicios AS total_servicios,
        @pagado_servicios AS pagado_servicios,
        @saldo_servicios AS saldo_servicios,
        @total_pendiente AS total_pendiente;
END;
GO

-- registrar saldo pendiente del alojamiento

CREATE OR ALTER PROCEDURE sp_Pago_RegistrarSaldoAlojamiento
    @id_reserva INT,
    @id_usuario INT,
    @metodo_pago VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @total_alojamiento DECIMAL(10,2),
        @pagado_alojamiento DECIMAL(10,2),
        @saldo_alojamiento DECIMAL(10,2),
        @estado_reserva VARCHAR(20);

    -- obtener datos de la reserva
    SELECT
        @total_alojamiento = total,
        @estado_reserva = estado
    FROM Reservas
    WHERE id_reserva = @id_reserva;

    IF @total_alojamiento IS NULL
        THROW 50021, 'La reserva no existe.', 1;

    -- el saldo se paga antes de realizar el check-in
    IF @estado_reserva <> 'CONFIRMADA'
        THROW 50022, 'La reserva debe estar CONFIRMADA para pagar el saldo del alojamiento.', 1;

    -- calcular cuánto del alojamiento ya fue pagado
    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago IN ('PAGO_INICIAL', 'SALDO_ALOJAMIENTO');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    IF @saldo_alojamiento <= 0
        THROW 50023, 'La reserva no tiene saldo de alojamiento pendiente.', 1;

    -- registrar exactamente el saldo restante
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
        @saldo_alojamiento,
        @metodo_pago,
        'APROBADO',
        'SALDO_ALOJAMIENTO'
    );

    SELECT
        SCOPE_IDENTITY() AS id_pago,
        @id_reserva AS id_reserva,
        @saldo_alojamiento AS monto_pagado,
        'SALDO_ALOJAMIENTO' AS tipo_pago;
END;
GO

-- registrar pago de servicios adicionales

CREATE OR ALTER PROCEDURE sp_Pago_RegistrarServicios
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
        THROW 50024, 'La reserva no tiene servicios adicionales para pagar.', 1;

    -- calcular lo ya pagado de servicios
    SELECT
        @pagado_servicios =
            ISNULL(SUM(monto), 0)
    FROM Pagos
    WHERE id_reserva = @id_reserva
      AND estado = 'APROBADO'
      AND tipo_pago = 'SERVICIOS_ADICIONALES';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_servicios <= 0
        THROW 50025, 'Los servicios adicionales ya están pagados.', 1;

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
        'APROBADO',
        'SERVICIOS_ADICIONALES'
    );

    SELECT
        SCOPE_IDENTITY() AS id_pago,
        @id_reserva AS id_reserva,
        @saldo_servicios AS monto_pagado,
        'SERVICIOS_ADICIONALES' AS tipo_pago;
END;
GO

-- reporte de ocupación de habitaciones

DROP PROCEDURE IF EXISTS dbo.sp_Reporte_Ocupacion;
GO


CREATE PROCEDURE dbo.sp_Reporte_Ocupacion
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_desde IS NULL OR @fecha_hasta IS NULL
        THROW 50040, 'Debe indicar ambas fechas.', 1;

    IF @fecha_hasta < @fecha_desde
        THROW 50040, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    IF @fecha_hasta = '9999-12-31'
        THROW 50041, 'La fecha hasta excede el rango permitido.', 1;

    DECLARE
        @fecha_fin_exclusiva DATE,
        @dias_periodo INT,
        @habitaciones_activas INT,
        @noches_disponibles INT,
        @noches_ocupadas INT,
        @porcentaje_ocupacion DECIMAL(10,2);

    SET @fecha_fin_exclusiva = DATEADD(DAY, 1, @fecha_hasta);

    SET @dias_periodo = DATEDIFF(
        DAY, @fecha_desde, @fecha_fin_exclusiva
    );

    SELECT
        @habitaciones_activas = COUNT(*)
    FROM dbo.Habitaciones
    WHERE activo = 1;

    SET @noches_disponibles =
        @habitaciones_activas * @dias_periodo;

    SELECT
        @noches_ocupadas = ISNULL(
            SUM(
                DATEDIFF(
                    DAY,
                    CASE
                        WHEN R.fecha_entrada < @fecha_desde
                            THEN @fecha_desde
                        ELSE R.fecha_entrada
                    END,
                    CASE
                        WHEN R.fecha_salida > @fecha_fin_exclusiva
                            THEN @fecha_fin_exclusiva
                        ELSE R.fecha_salida
                    END
                )
            ),
            0
        )
    FROM dbo.Reservas R
    INNER JOIN dbo.DetalleReservas DR
        ON R.id_reserva = DR.id_reserva
    INNER JOIN dbo.Habitaciones H
        ON DR.id_habitacion = H.id_habitacion
    WHERE H.activo = 1
      AND R.estado IN ('CONFIRMADA', 'CHECKIN', 'FINALIZADA')
      AND R.fecha_entrada < @fecha_fin_exclusiva
      AND R.fecha_salida > @fecha_desde;

    IF @noches_disponibles > 0
        SET @porcentaje_ocupacion = ROUND(
            (@noches_ocupadas * 100.0) / @noches_disponibles,
            2
        );
    ELSE
        SET @porcentaje_ocupacion = 0;

    SELECT
        @fecha_desde AS fecha_desde,
        @fecha_hasta AS fecha_hasta,
        @habitaciones_activas AS habitaciones_activas,
        @noches_disponibles AS noches_disponibles,
        @noches_ocupadas AS noches_ocupadas,
        @porcentaje_ocupacion AS porcentaje_ocupacion;
END;
GO

-- reporte de ingresos

DROP PROCEDURE IF EXISTS dbo.sp_Reporte_Ingresos;
GO


CREATE PROCEDURE dbo.sp_Reporte_Ingresos
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_desde IS NULL OR @fecha_hasta IS NULL
        THROW 50042, 'Debe indicar ambas fechas.', 1;

    IF @fecha_hasta < @fecha_desde
        THROW 50042, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    -- 1. Resumen de pagos aprobados
    SELECT
        @fecha_desde AS fecha_desde,
        @fecha_hasta AS fecha_hasta,
        COUNT(*) AS cantidad_pagos,
        ISNULL(SUM(P.monto), 0) AS total_ingresos,
        ISNULL(AVG(P.monto), 0) AS promedio_pago,
        COUNT(DISTINCT P.id_reserva) AS cantidad_reservas
    FROM dbo.Pagos P
    WHERE P.estado = 'APROBADO'
      AND P.fecha_pago >= @fecha_desde
      AND P.fecha_pago < DATEADD(DAY, 1, CAST(@fecha_hasta AS DATETIME));

    -- 2. Ingresos por método de pago
    SELECT
        P.metodo_pago,
        COUNT(*) AS cantidad_pagos,
        SUM(P.monto) AS total_ingresos
    FROM dbo.Pagos P
    WHERE P.estado = 'APROBADO'
      AND P.fecha_pago >= @fecha_desde
      AND P.fecha_pago < DATEADD(DAY, 1, CAST(@fecha_hasta AS DATETIME))
    GROUP BY P.metodo_pago
    ORDER BY total_ingresos DESC;

    -- 3. Ingresos por tipo de pago
    SELECT
        P.tipo_pago,
        COUNT(*) AS cantidad_pagos,
        SUM(P.monto) AS total_ingresos
    FROM dbo.Pagos P
    WHERE P.estado = 'APROBADO'
      AND P.fecha_pago >= @fecha_desde
      AND P.fecha_pago < DATEADD(DAY, 1, CAST(@fecha_hasta AS DATETIME))
    GROUP BY P.tipo_pago
    ORDER BY total_ingresos DESC;

    -- 4. Pagos aprobados de reservas temporales
    SELECT
        RT.estado AS estado_reserva_temporal,
        COUNT(*) AS cantidad_pagos,
        ISNULL(SUM(P.monto), 0) AS importe_total
    FROM dbo.Pagos P
    INNER JOIN dbo.ReservasTemporales RT
        ON P.id_reserva_temporal = RT.id_reserva_temporal
    WHERE P.estado = 'APROBADO'
      AND P.id_reserva IS NULL
      AND P.fecha_pago >= @fecha_desde
      AND P.fecha_pago < DATEADD(DAY, 1, CAST(@fecha_hasta AS DATETIME))
    GROUP BY RT.estado
    ORDER BY RT.estado;
END;
GO


-- reporte de estadísticas de reservas

DROP PROCEDURE IF EXISTS dbo.sp_Reporte_EstadisticasReservas;
GO

CREATE PROCEDURE dbo.sp_Reporte_EstadisticasReservas
   @fecha_desde DATE,
   @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_hasta < @fecha_desde
        THROW 50043, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    -- resumen general
    SELECT
        COUNT(*) AS total_reservas,

        SUM(CASE WHEN estado = 'CONFIRMADA' THEN 1 ELSE 0 END) AS confirmadas,

        SUM(CASE WHEN estado = 'CHECKIN' THEN 1 ELSE 0 END) AS en_checkin,

        SUM(CASE WHEN estado = 'FINALIZADA' THEN 1 ELSE 0 END) AS finalizadas,

        SUM(CASE WHEN estado = 'CANCELADA' THEN 1 ELSE 0 END) AS canceladas,

        CAST(
            CASE
                WHEN COUNT(*) = 0 THEN 0
                ELSE
                    SUM(CASE WHEN estado = 'CANCELADA' THEN 1 ELSE 0 END) * 100.0
                    / COUNT(*)
            END
            AS DECIMAL(10,2)
        ) AS porcentaje_cancelacion

    FROM Reservas
    WHERE fecha_entrada BETWEEN @fecha_desde AND @fecha_hasta;


    -- reservas por tipo de habitación
    SELECT
        TH.nombre AS tipo_habitacion,
        COUNT(DISTINCT R.id_reserva) AS cantidad_reservas,
        SUM(DATEDIFF(DAY, R.fecha_entrada, R.fecha_salida)) AS noches_reservadas
    FROM Reservas R
    INNER JOIN DetalleReservas DR
        ON R.id_reserva = DR.id_reserva
    INNER JOIN Habitaciones H
        ON DR.id_habitacion = H.id_habitacion
    INNER JOIN TiposHabitacion TH
        ON H.tipo_habitacion = TH.id_tipoHabitacion
    WHERE R.fecha_entrada BETWEEN @fecha_desde AND @fecha_hasta
      AND R.estado <> 'CANCELADA'
    GROUP BY TH.nombre
    ORDER BY cantidad_reservas DESC;


    -- demanda por mes
    SELECT
        YEAR(fecha_entrada) AS anio,
        MONTH(fecha_entrada) AS mes,
        COUNT(*) AS cantidad_reservas
    FROM Reservas
    WHERE fecha_entrada BETWEEN @fecha_desde AND @fecha_hasta
    GROUP BY
        YEAR(fecha_entrada),
        MONTH(fecha_entrada)
    ORDER BY
        anio,
        mes;
END;
GO

-- reporte de habitaciones más utilizadas

DROP PROCEDURE IF EXISTS dbo.sp_Reporte_HabitacionesMasUtilizadas;
GO


CREATE PROCEDURE dbo.sp_Reporte_HabitacionesMasUtilizadas
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_desde IS NULL OR @fecha_hasta IS NULL
        THROW 50044, 'Debe indicar ambas fechas.', 1;

    IF @fecha_hasta < @fecha_desde
        THROW 50044, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    DECLARE @fecha_fin_exclusiva DATE;

    SET @fecha_fin_exclusiva = DATEADD(DAY, 1, @fecha_hasta);

    SELECT
        H.id_habitacion,
        H.numero AS numero_habitacion,
        TH.nombre AS tipo_habitacion,

        COUNT(DISTINCT R.id_reserva) AS cantidad_reservas,

        SUM(
            DATEDIFF(
                DAY,
                CASE
                    WHEN R.fecha_entrada < @fecha_desde
                        THEN @fecha_desde
                    ELSE R.fecha_entrada
                END,
                CASE
                    WHEN R.fecha_salida > @fecha_fin_exclusiva
                        THEN @fecha_fin_exclusiva
                    ELSE R.fecha_salida
                END
            )
        ) AS noches_ocupadas,

        SUM(
            DATEDIFF(
                DAY,
                CASE
                    WHEN R.fecha_entrada < @fecha_desde
                        THEN @fecha_desde
                    ELSE R.fecha_entrada
                END,
                CASE
                    WHEN R.fecha_salida > @fecha_fin_exclusiva
                        THEN @fecha_fin_exclusiva
                    ELSE R.fecha_salida
                END
            ) * DR.precio_noche
        ) AS ingresos_generados

    FROM dbo.DetalleReservas DR

    INNER JOIN dbo.Reservas R
        ON DR.id_reserva = R.id_reserva

    INNER JOIN dbo.Habitaciones H
        ON DR.id_habitacion = H.id_habitacion

    INNER JOIN dbo.TiposHabitacion TH
        ON H.tipo_habitacion = TH.id_tipoHabitacion

    WHERE R.estado IN ('CONFIRMADA', 'CHECKIN', 'FINALIZADA')
      AND R.fecha_entrada < @fecha_fin_exclusiva
      AND R.fecha_salida > @fecha_desde

    GROUP BY
        H.id_habitacion,
        H.numero,
        TH.nombre

    ORDER BY
        noches_ocupadas DESC,
        cantidad_reservas DESC;
END;
GO


-- reporte de servicios más utilizados

DROP PROCEDURE IF EXISTS dbo.sp_Reporte_ServiciosMasUtilizados;
GO

CREATE PROCEDURE dbo.sp_Reporte_ServiciosMasUtilizados
  @fecha_desde DATE,
  @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_hasta < @fecha_desde
        THROW 50045, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    SELECT
        S.id_servicio,
        S.nombre AS servicio,

        SUM(DS.cantidad) AS cantidad_total_consumida,

        COUNT(DISTINCT DS.id_reserva) AS cantidad_reservas,

        CAST(
            AVG(DS.precio_unitario)
            AS DECIMAL(10,2)
        ) AS precio_promedio_historico,

        SUM(
            DS.cantidad * DS.precio_unitario
        ) AS ingresos_generados

    FROM DetalleServicios DS

    INNER JOIN Servicios S
        ON DS.id_servicios = S.id_servicio

    INNER JOIN Reservas R
        ON DS.id_reserva = R.id_reserva

    WHERE R.fecha_entrada BETWEEN @fecha_desde AND @fecha_hasta
      AND R.estado <> 'CANCELADA'

    GROUP BY
        S.id_servicio,
        S.nombre

    ORDER BY
        cantidad_total_consumida DESC,
        ingresos_generados DESC;
END;
GO

---obtiene los roles activos del hotel.
SELECT
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Roles';
GO

-- 3. Mostrar los roles registrados
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
BEGIN
    SELECT *
    FROM dbo.Roles;
END
ELSE
BEGIN
    PRINT 'No se encontró la tabla dbo.Roles.';
END;
GO





-- ==========================================
-- PROCEDIMIENTO: LISTAR ROLES ACTIVOS
-- ==========================================

IF OBJECT_ID('dbo.sp_Rol_Listar', 'P') IS NULL
BEGIN
    EXEC('CREATE PROCEDURE dbo.sp_Rol_Listar AS BEGIN SET NOCOUNT ON; END');
END;
GO

ALTER PROCEDURE dbo.sp_Rol_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_rol,
        nombre_rol,
        descripcion
    FROM dbo.Roles
    WHERE activo = 1
    ORDER BY nombre_rol;
END;
GO

-- Comprobar que funciona
EXEC dbo.sp_Rol_Listar;
GO

-- ==========================================
-- PROCEDIMIENTO: REACTIVAR HUÉSPED
-- ==========================================

CREATE OR ALTER PROCEDURE dbo.sp_Huesped_Reactivar
    @id_huesped INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Huespedes
        WHERE id_huesped = @id_huesped
    )
    BEGIN
        THROW 50040, 'El huésped no existe.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.Huespedes
        WHERE id_huesped = @id_huesped
          AND activo = 1
    )
    BEGIN
        THROW 50041, 'El huésped ya está activo.', 1;
    END;

    UPDATE dbo.Huespedes
    SET activo = 1
    WHERE id_huesped = @id_huesped;
END;
GO