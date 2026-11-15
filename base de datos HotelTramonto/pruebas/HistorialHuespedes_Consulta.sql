-- Solo lectura de datos existentes. Instalar primero Actualizacion_HistorialHuespedes.sql.
-- Ejecutar cada caso en HotelTramontoDB_Pruebas de PRUEBAS. No crea reservas.
IF DB_NAME() <> N'HotelTramontoDB_Pruebas'
    THROW 51000, 'Base incorrecta.', 1;

CREATE TABLE #Historial (
 id_reserva INT, id_huesped INT, dni VARCHAR(15), nombre VARCHAR(50), apellido VARCHAR(50),
 fecha_creacion DATETIME, entrada_prevista DATE, salida_prevista DATE, estado VARCHAR(20),
 checkin_reserva DATETIME, checkout_reserva DATETIME,
 id_habitacion INT NULL, numero_habitacion_actual VARCHAR(10) NULL,
 tipo_habitacion_actual VARCHAR(50) NULL
);
DECLARE @id INT;
DECLARE huespedes CURSOR LOCAL FAST_FORWARD FOR SELECT id_huesped FROM dbo.Huespedes;
OPEN huespedes;
FETCH NEXT FROM huespedes INTO @id;
WHILE @@FETCH_STATUS = 0
BEGIN
 DELETE FROM #Historial;
 INSERT INTO #Historial EXEC dbo.sp_Huesped_HistorialReservas @id;
 IF EXISTS (SELECT 1 FROM #Historial WHERE id_huesped <> @id)
    THROW 51002, 'Se devolvió otro huésped.', 1;
 IF EXISTS (SELECT id_reserva, id_habitacion FROM #Historial
            GROUP BY id_reserva, id_habitacion HAVING COUNT(*) > 1)
    THROW 51003, 'Filas duplicadas.', 1;
 IF (SELECT COUNT(*) FROM #Historial) <>
    (SELECT COUNT(*) FROM dbo.Reservas r
     LEFT JOIN dbo.DetalleReservas d ON d.id_reserva = r.id_reserva
     WHERE r.id_huesped = @id)
    THROW 51004, 'Cantidad incorrecta de habitaciones/reservas.', 1;
 IF EXISTS (SELECT id_reserva, estado, fecha_checkin, fecha_checkout
            FROM dbo.Reservas WHERE id_huesped = @id
            EXCEPT SELECT id_reserva, estado, checkin_reserva, checkout_reserva FROM #Historial)
    THROW 51005, 'Reserva o estado histórico omitido.', 1;
 IF EXISTS (
    SELECT r.id_reserva, d.id_habitacion, hab.numero, t.nombre
    FROM dbo.Reservas r
    LEFT JOIN dbo.DetalleReservas d ON d.id_reserva = r.id_reserva
    LEFT JOIN dbo.Habitaciones hab ON hab.id_habitacion = d.id_habitacion
    LEFT JOIN dbo.TiposHabitacion t ON t.id_tipoHabitacion = hab.tipo_habitacion
    WHERE r.id_huesped = @id
    EXCEPT SELECT id_reserva, id_habitacion, numero_habitacion_actual,
                  tipo_habitacion_actual FROM #Historial)
    THROW 51007, 'Habitación o tipo incorrecto.', 1;
 FETCH NEXT FROM huespedes INTO @id;
END;
CLOSE huespedes;
DEALLOCATE huespedes;
DROP TABLE #Historial;

BEGIN TRY
 EXEC dbo.sp_Huesped_HistorialReservas 0;
 THROW 51006, 'Se aceptó ID cero.', 1;
END TRY
BEGIN CATCH
 IF ERROR_NUMBER() <> 51001 THROW;
END CATCH;
BEGIN TRY
 EXEC dbo.sp_Huesped_HistorialReservas -1;
 THROW 51006, 'Se aceptó ID negativo.', 1;
END TRY
BEGIN CATCH
 IF ERROR_NUMBER() <> 51001 THROW;
END CATCH;
BEGIN TRY
 EXEC dbo.sp_Huesped_HistorialReservas NULL;
 THROW 51006, 'Se aceptó ID nulo.', 1;
END TRY
BEGIN CATCH
 IF ERROR_NUMBER() <> 51001 THROW;
END CATCH;

-- Cobertura real disponible: ausencia de filas en una categoría significa PENDIENTE.
SELECT h.id_huesped, COUNT(r.id_reserva) AS cantidad_reservas
FROM dbo.Huespedes h LEFT JOIN dbo.Reservas r ON r.id_huesped = h.id_huesped
GROUP BY h.id_huesped;
SELECT r.estado, COUNT(*) AS cantidad FROM dbo.Reservas r GROUP BY r.estado;
SELECT id_reserva, COUNT(*) AS habitaciones FROM dbo.DetalleReservas
GROUP BY id_reserva HAVING COUNT(*) > 1;
PRINT 'Comprobaciones completadas para los huéspedes existentes; revisar cobertura.';
