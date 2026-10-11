USE HotelTramontoDB;
GO

CREATE INDEX IX_Usuarios_IdRol
ON Usuarios(id_rol);

CREATE INDEX IX_Habitaciones_TipoHabitacion
ON Habitaciones(tipo_habitacion);

CREATE INDEX IX_Reservas_IdHuesped
ON Reservas(id_huesped);

CREATE INDEX IX_Reservas_IdUsuario
ON Reservas(id_usuario);

CREATE INDEX IX_Reservas_Fechas_Estado
ON Reservas(fecha_entrada, fecha_salida, estado);

CREATE INDEX IX_DetalleReservas_IdHabitacion
ON DetalleReservas(id_habitacion);

CREATE INDEX IX_Pagos_IdReserva
ON Pagos(id_reserva);

CREATE INDEX IX_Pagos_IdUsuario
ON Pagos(id_usuario);

CREATE INDEX IX_DetalleServicios_IdReserva
ON DetalleServicios(id_reserva);

CREATE INDEX IX_DetalleServicios_IdServicios
ON DetalleServicios(id_servicios);

CREATE INDEX IX_ReservasTemporales_IdHuesped
ON ReservasTemporales(id_huesped);

CREATE INDEX IX_ReservasTemporales_IdHabitacion
ON ReservasTemporales(id_habitacion);

CREATE INDEX IX_ReservasTemporales_FechaExpiracion
ON ReservasTemporales(fecha_expiracion);
GO

SELECT
    OBJECT_NAME(object_id) AS Tabla,
    name AS Indice
FROM sys.indexes
WHERE name IS NOT NULL
ORDER BY Tabla, Indice;

SELECT COUNT(*) AS CantidadIndicesCreados
FROM sys.indexes
WHERE name LIKE 'IX_%';


SELECT
    OBJECT_NAME(object_id) AS Tabla,
    name AS Indice
FROM sys.indexes
WHERE name LIKE 'IX_%'
ORDER BY Tabla, Indice;