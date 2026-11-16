ALTER PROCEDURE dbo.sp_Reporte_ServiciosMasUtilizados
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_desde IS NULL OR @fecha_hasta IS NULL
        THROW 50045, 'Las fechas son obligatorias.', 1;

    IF @fecha_hasta < @fecha_desde
        THROW 50046, 'La fecha hasta no puede ser anterior a la fecha desde.', 1;

    SELECT
        S.id_servicio,
        S.nombre AS servicio,

        SUM(DS.cantidad) AS cantidad_total_consumida,

        COUNT(DISTINCT DS.id_reserva) AS cantidad_reservas,

        CAST(
            SUM(DS.cantidad * DS.precio_unitario) /
            NULLIF(SUM(DS.cantidad), 0)
            AS DECIMAL(10,2)
        ) AS precio_promedio_historico,

        SUM(DS.cantidad * DS.precio_unitario) AS ingresos_generados

    FROM dbo.DetalleServicios DS

    INNER JOIN dbo.Servicios S
        ON DS.id_servicios = S.id_servicio

    INNER JOIN dbo.Reservas R
        ON DS.id_reserva = R.id_reserva

    WHERE DS.fecha_consumo >= @fecha_desde
      AND DS.fecha_consumo < DATEADD(DAY, 1, @fecha_hasta)
      AND R.estado <> 'CANCELADA'

    GROUP BY
        S.id_servicio,
        S.nombre

    ORDER BY
        cantidad_total_consumida DESC,
        ingresos_generados DESC;
END;