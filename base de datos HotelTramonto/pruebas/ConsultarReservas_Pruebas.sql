/* Solo lectura. Ejecutar en SSMS, conectado a LAPTOP-5TSJJUP8\SQLEXPRESS,
   seleccionando HotelTramontoDB_Pruebas. No instala ni ejecuta procedimientos.
   NULL lista todas las reservas; un ID limita los resultados a esa reserva.
   Los resultados pueden contener nombres: conservarlos de forma privada.
   Consultas sucesivas: con actividad concurrente no constituyen una instantánea.
*/
SET NOCOUNT ON;

IF DB_NAME() <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51000, 'Consulta cancelada: seleccionar HotelTramontoDB_Pruebas.', 1;
    RETURN;
END;
IF COALESCE(CONVERT(nvarchar(128), SERVERPROPERTY('MachineName')), N'') <> N'LAPTOP-5TSJJUP8'
   OR COALESCE(CONVERT(nvarchar(128), SERVERPROPERTY('InstanceName')), N'') <> N'SQLEXPRESS'
BEGIN
    THROW 51001, 'Consulta cancelada: se requiere LAPTOP-5TSJJUP8\SQLEXPRESS.', 1;
    RETURN;
END;

DECLARE @IdReserva int = NULL;

-- 1. Destino e identidad efectivos; sysadmin/db_owner NO prueba permisos mínimos.
SELECT CONVERT(nvarchar(128), SERVERPROPERTY('ServerName')) AS servidor,
       DB_NAME() AS base_datos, ORIGINAL_LOGIN() AS login_original,
       USER_NAME() AS usuario_base,
       IS_SRVROLEMEMBER('sysadmin') AS es_sysadmin,
       IS_MEMBER('db_owner') AS es_db_owner;

-- 2. Una fila por reserva definitiva. Agregados independientes evitan multiplicar importes.
;WITH Habitaciones AS (
    SELECT d.id_reserva, COUNT_BIG(*) AS cantidad_habitaciones,
           STRING_AGG(CONVERT(nvarchar(max), CONCAT(
               h.numero, N' [ID ', h.id_habitacion, N'] — ', t.nombre)), N'; ')
               WITHIN GROUP (ORDER BY h.numero, h.id_habitacion) AS habitaciones
    FROM dbo.DetalleReservas AS d
    JOIN dbo.Habitaciones AS h ON h.id_habitacion = d.id_habitacion
    JOIN dbo.TiposHabitacion AS t ON t.id_tipoHabitacion = h.tipo_habitacion
    GROUP BY d.id_reserva
), Pagos AS (
    SELECT p.id_reserva, COUNT_BIG(*) AS cantidad_pagos,
           SUM(CASE WHEN p.estado = 'APROBADO'
                     AND p.tipo_pago IN ('PAGO_INICIAL', 'SALDO_ALOJAMIENTO')
                    THEN CONVERT(decimal(38,2), p.monto) ELSE 0 END) AS alojamiento_aprobado,
           SUM(CASE WHEN p.estado = 'APROBADO' AND p.tipo_pago = 'SERVICIOS_ADICIONALES'
                    THEN CONVERT(decimal(38,2), p.monto) ELSE 0 END) AS servicios_aprobados,
           SUM(CASE WHEN p.estado = 'PENDIENTE'
                    THEN CONVERT(decimal(38,2), p.monto) ELSE 0 END) AS pagos_pendientes,
           SUM(CASE WHEN p.estado = 'REEMBOLSADO'
                    THEN CONVERT(decimal(38,2), p.monto) ELSE 0 END) AS pagos_reembolsados
    FROM dbo.Pagos AS p
    WHERE p.id_reserva IS NOT NULL
    GROUP BY p.id_reserva
), Consumos AS (
    SELECT d.id_reserva, COUNT_BIG(*) AS cantidad_consumos,
           SUM(CONVERT(decimal(19,0), d.cantidad) * CONVERT(decimal(18,2), d.precio_unitario)) AS total_consumido
    FROM dbo.DetalleServicios AS d
    GROUP BY d.id_reserva
)
SELECT r.id_reserva, r.id_huesped,
       CONCAT(h.apellido, N', ', h.nombre) AS huesped, r.estado,
       r.fecha_entrada, r.fecha_salida, r.fecha_checkin, r.fecha_checkout,
       r.modalidad_pago,
       COALESCE(ha.cantidad_habitaciones, 0) AS cantidad_habitaciones,
       COALESCE(ha.habitaciones, N'Sin habitaciones asociadas') AS habitaciones,
       r.total AS total_alojamiento,
       COALESCE(p.cantidad_pagos, 0) AS cantidad_pagos,
       COALESCE(p.alojamiento_aprobado, 0) AS alojamiento_aprobado,
       CONVERT(decimal(28,2), r.total) - CONVERT(decimal(28,2), COALESCE(p.alojamiento_aprobado, 0)) AS diferencia_alojamiento,
       COALESCE(c.cantidad_consumos, 0) AS cantidad_consumos,
       COALESCE(c.total_consumido, 0) AS total_consumido,
       COALESCE(p.servicios_aprobados, 0) AS servicios_aprobados,
       CONVERT(decimal(28,2), COALESCE(c.total_consumido, 0)) - CONVERT(decimal(28,2), COALESCE(p.servicios_aprobados, 0)) AS diferencia_servicios,
       COALESCE(p.pagos_pendientes, 0) AS pagos_pendientes,
       COALESCE(p.pagos_reembolsados, 0) AS pagos_reembolsados,
       CASE WHEN r.id_reserva = 1
            THEN N'Histórico inconsistente documentado: conservar; no usar para pruebas'
            ELSE N'Origen de prueba no acreditado: no modificar sin identificar el caso'
       END AS advertencia
FROM dbo.Reservas AS r
LEFT JOIN dbo.Huespedes AS h ON h.id_huesped = r.id_huesped
LEFT JOIN Habitaciones AS ha ON ha.id_reserva = r.id_reserva
LEFT JOIN Pagos AS p ON p.id_reserva = r.id_reserva
LEFT JOIN Consumos AS c ON c.id_reserva = r.id_reserva
WHERE @IdReserva IS NULL OR r.id_reserva = @IdReserva
ORDER BY r.id_reserva DESC;

-- 3. Pagos definitivos: incluye TODOS los estados, sin ocultar históricos.
SELECT p.id_reserva, p.id_pago, p.tipo_pago, p.estado, p.monto,
       p.metodo_pago, p.fecha_pago, p.id_usuario, p.id_operacion
FROM dbo.Pagos AS p
WHERE p.id_reserva IS NOT NULL
  AND (@IdReserva IS NULL OR p.id_reserva = @IdReserva)
ORDER BY p.id_reserva DESC, p.fecha_pago, p.id_pago;

-- 4. Asociación precisa de habitaciones; tipo y número son los del catálogo actual.
SELECT d.id_reserva, d.id_habitacion, h.numero AS numero_habitacion,
       t.nombre AS tipo_habitacion_actual, d.precio_noche AS precio_noche_reservado,
       d.cantidad_personas, h.estado AS estado_habitacion_actual
FROM dbo.DetalleReservas AS d
JOIN dbo.Habitaciones AS h ON h.id_habitacion = d.id_habitacion
JOIN dbo.TiposHabitacion AS t ON t.id_tipoHabitacion = h.tipo_habitacion
WHERE @IdReserva IS NULL OR d.id_reserva = @IdReserva
ORDER BY d.id_reserva DESC, h.numero, h.id_habitacion;

-- 5. Consumos con su precio histórico y habitación; NULL conserva legado sin asignación.
SELECT d.id_reserva, d.id_detalle_servicio, d.id_habitacion,
       h.numero AS numero_habitacion, d.id_servicios,
       s.nombre AS servicio_actual, d.cantidad, d.precio_unitario,
       CONVERT(decimal(19,0), d.cantidad) * CONVERT(decimal(18,2), d.precio_unitario) AS subtotal,
       d.id_operacion
FROM dbo.DetalleServicios AS d
LEFT JOIN dbo.Habitaciones AS h ON h.id_habitacion = d.id_habitacion
LEFT JOIN dbo.Servicios AS s ON s.id_servicio = d.id_servicios
WHERE @IdReserva IS NULL OR d.id_reserva = @IdReserva
ORDER BY d.id_reserva DESC, d.id_detalle_servicio;

-- 6. Temporales se muestran aparte: su ID NO debe ingresarse como reserva definitiva en Pagos.
SELECT t.id_reserva_temporal, t.id_reserva AS id_reserva_definitiva,
       t.id_huesped, CONCAT(h.apellido, N', ', h.nombre) AS huesped,
       t.estado AS estado_temporal, t.id_habitacion, ha.numero AS numero_habitacion,
       t.fecha_entrada, t.fecha_salida, t.fecha_expiracion,
       p.id_pago, p.tipo_pago, p.estado AS estado_pago, p.monto, p.metodo_pago
FROM dbo.ReservasTemporales AS t
LEFT JOIN dbo.Huespedes AS h ON h.id_huesped = t.id_huesped
LEFT JOIN dbo.Habitaciones AS ha ON ha.id_habitacion = t.id_habitacion
LEFT JOIN dbo.Pagos AS p ON p.id_reserva_temporal = t.id_reserva_temporal
WHERE @IdReserva IS NULL OR t.id_reserva = @IdReserva
ORDER BY t.id_reserva_temporal DESC, p.id_pago;

-- Diferencias negativas se conservan para detectar sobrepagos: no se fuerzan a cero.
-- Ninguna fila se clasifica automáticamente como apta para nuevas escrituras.
