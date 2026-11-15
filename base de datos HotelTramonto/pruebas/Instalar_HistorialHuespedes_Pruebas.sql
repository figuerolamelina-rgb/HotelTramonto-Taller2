-- Instalador de PRUEBAS, autocontenido e idempotente. Revisar también la instancia.
-- La guarda y el DDL están en el mismo lote: una base incorrecta no instala nada.
IF DB_NAME() <> N'HotelTramontoDB_Pruebas'
    THROW 51000, 'Base incorrecta: seleccione HotelTramontoDB_Pruebas.', 1;
ELSE
    EXEC sys.sp_executesql N'
CREATE OR ALTER PROCEDURE dbo.sp_Huesped_HistorialReservas
    @id_huesped INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @id_huesped IS NULL OR @id_huesped <= 0
        THROW 51001, ''El identificador del huésped debe ser positivo.'', 1;

    SELECT r.id_reserva, h.id_huesped, h.dni, h.nombre, h.apellido,
           r.fecha_creacion, r.fecha_entrada AS entrada_prevista,
           r.fecha_salida AS salida_prevista, r.estado,
           r.fecha_checkin AS checkin_reserva, r.fecha_checkout AS checkout_reserva,
           d.id_habitacion, hab.numero AS numero_habitacion_actual,
           t.nombre AS tipo_habitacion_actual
    FROM dbo.Reservas AS r
    INNER JOIN dbo.Huespedes AS h ON h.id_huesped = r.id_huesped
    LEFT JOIN dbo.DetalleReservas AS d ON d.id_reserva = r.id_reserva
    LEFT JOIN dbo.Habitaciones AS hab ON hab.id_habitacion = d.id_habitacion
    LEFT JOIN dbo.TiposHabitacion AS t ON t.id_tipoHabitacion = hab.tipo_habitacion
    WHERE r.id_huesped = @id_huesped
    ORDER BY r.fecha_creacion DESC, r.id_reserva DESC, hab.numero, d.id_habitacion;
END;';
GO
