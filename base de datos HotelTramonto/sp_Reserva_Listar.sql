
CREATE OR ALTER PROCEDURE dbo.sp_Reserva_Listar
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
        R.fecha_checkout,

        ISNULL(
            STRING_AGG(
                CAST(HA.numero AS VARCHAR(20)),
                ', '
            ),
            'Sin asignar'
        ) AS habitaciones,

        -- Saldo pendiente del alojamiento
        R.total - ISNULL((
            SELECT SUM(
                CASE
                    WHEN MP.tipo_movimiento = 'COBRO'
                        THEN MP.monto
                    WHEN MP.tipo_movimiento = 'DEVOLUCION'
                        THEN -MP.monto
                    ELSE 0
                END
            )
            FROM dbo.MovimientosPagos MP
            INNER JOIN dbo.Pagos P
                ON MP.id_pago = P.id_pago
            WHERE P.id_reserva = R.id_reserva
              AND P.tipo_pago IN (
                  'PAGO_INICIAL',
                  'SALDO_ALOJAMIENTO'
              )
        ), 0) AS saldo_alojamiento,

        -- Saldo pendiente de servicios
        ISNULL((
            SELECT SUM(DS.cantidad * DS.precio_unitario)
            FROM dbo.DetalleServicios DS
            WHERE DS.id_reserva = R.id_reserva
        ), 0)
        -
        ISNULL((
            SELECT SUM(
                CASE
                    WHEN MP.tipo_movimiento = 'COBRO'
                        THEN MP.monto
                    WHEN MP.tipo_movimiento = 'DEVOLUCION'
                        THEN -MP.monto
                    ELSE 0
                END
            )
            FROM dbo.MovimientosPagos MP
            INNER JOIN dbo.Pagos P
                ON MP.id_pago = P.id_pago
            WHERE P.id_reserva = R.id_reserva
              AND P.tipo_pago = 'SERVICIOS_ADICIONALES'
        ), 0) AS saldo_servicios

    FROM dbo.Reservas R
    INNER JOIN dbo.Huespedes H
        ON R.id_huesped = H.id_huesped
    LEFT JOIN dbo.Usuarios U
        ON R.id_usuario = U.id_usuario
    LEFT JOIN dbo.DetalleReservas DR
        ON R.id_reserva = DR.id_reserva
    LEFT JOIN dbo.Habitaciones HA
        ON DR.id_habitacion = HA.id_habitacion

    GROUP BY
        R.id_reserva,
        R.id_huesped,
        H.nombre,
        H.apellido,
        R.id_usuario,
        U.nombre,
        U.apellido,
        R.fecha_entrada,
        R.fecha_salida,
        R.estado,
        R.total,
        R.observaciones,
        R.fecha_creacion,
        R.fecha_checkin,
        R.fecha_checkout

    ORDER BY R.fecha_creacion DESC;
END;
