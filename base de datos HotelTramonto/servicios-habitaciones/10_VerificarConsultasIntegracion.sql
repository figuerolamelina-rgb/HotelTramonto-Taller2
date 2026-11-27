-- Incremento de integracion Iara/Melina. Preparado, no ejecutado automaticamente.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51900, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
IF COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')),N'')<>N'LAPTOP-5TSJJUP8'
 OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')),N'')<>N'SQLEXPRESS'
 THROW 51901, 'Instancia no autorizada.', 1;
IF @@TRANCOUNT<>0 THROW 51902, 'Ejecutar sin transaccion externa.', 1;
DECLARE @nuevo0 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Habitacion_ListarConReservaCheckIn
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.id_reserva,h.id_habitacion,h.numero AS numero_habitacion,h.numero,h.piso,
           hu.nombre AS nombre_huesped,hu.apellido AS apellido_huesped
    FROM dbo.Reservas r
    JOIN dbo.DetalleReservas dr ON dr.id_reserva=r.id_reserva
    JOIN dbo.Habitaciones h ON h.id_habitacion=dr.id_habitacion
    JOIN dbo.Huespedes hu ON hu.id_huesped=r.id_huesped
    WHERE r.estado=''CHECKIN'' AND r.fecha_checkout IS NULL
    ORDER BY h.numero,r.id_reserva;
END;';
DECLARE @nuevo1 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_ObtenerComprobante
    @id_pago INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @id_pago IS NULL OR @id_pago<=0 THROW 51910, ''El pago debe ser positivo.'', 1;
    SELECT p.id_pago,p.id_reserva,p.monto,p.estado,p.metodo_pago,p.tipo_pago,p.fecha_pago,
           h.nombre AS nombre_huesped,h.apellido AS apellido_huesped,h.dni,
           COALESCE(u.nombre,''No disponible'') AS nombre_usuario,COALESCE(u.apellido,'''') AS apellido_usuario
    FROM dbo.Pagos p
    JOIN dbo.Reservas r ON r.id_reserva=p.id_reserva
    JOIN dbo.Huespedes h ON h.id_huesped=r.id_huesped
    LEFT JOIN dbo.Usuarios u ON u.id_usuario=p.id_usuario
    WHERE p.id_pago=@id_pago AND p.estado=''APROBADO'';
END;';
DECLARE @nuevo2 nvarchar(max)=N'CREATE OR ALTER PROCEDURE dbo.sp_Reporte_ServiciosMasUtilizados
    @fecha_desde DATE,
    @fecha_hasta DATE
AS
BEGIN
    SET NOCOUNT ON;
    IF @fecha_desde IS NULL OR @fecha_hasta IS NULL OR @fecha_hasta<@fecha_desde
        THROW 50045, ''El rango de fechas no es valido.'', 1;
    IF @fecha_hasta=''9999-12-31'' THROW 50046, ''La fecha hasta excede el rango permitido.'', 1;
    SELECT s.id_servicio,s.nombre AS servicio,
           CASE WHEN ds.fecha_consumo IS NULL THEN N''Entrada prevista; fecha de consumo desconocida''
                ELSE N''Fecha de consumo'' END AS criterio_fecha,
           SUM(CONVERT(BIGINT,ds.cantidad)) AS cantidad_total_consumida,
           COUNT(DISTINCT ds.id_reserva) AS cantidad_reservas,
           CONVERT(DECIMAL(18,2),SUM(CONVERT(DECIMAL(19,0),ds.cantidad)*ds.precio_unitario)
                    /NULLIF(SUM(CONVERT(DECIMAL(19,0),ds.cantidad)),0)) AS precio_promedio_historico,
           SUM(CONVERT(DECIMAL(19,0),ds.cantidad)*ds.precio_unitario) AS ingresos_generados
    FROM dbo.DetalleServicios ds
    JOIN dbo.Servicios s ON s.id_servicio=ds.id_servicios
    JOIN dbo.Reservas r ON r.id_reserva=ds.id_reserva
    WHERE r.estado<>''CANCELADA'' AND
          ((ds.fecha_consumo>=@fecha_desde AND ds.fecha_consumo<DATEADD(DAY,1,@fecha_hasta))
           OR (ds.fecha_consumo IS NULL AND r.fecha_entrada BETWEEN @fecha_desde AND @fecha_hasta))
    GROUP BY s.id_servicio,s.nombre,CASE WHEN ds.fecha_consumo IS NULL
             THEN N''Entrada prevista; fecha de consumo desconocida'' ELSE N''Fecha de consumo'' END
    ORDER BY cantidad_total_consumida DESC,ingresos_generados DESC;
END;';
DECLARE @error nvarchar(2048);
-- INICIO comparación del paso 10
;WITH contratos(nombre, esperada) AS (
    SELECT N'dbo.sp_Habitacion_ListarConReservaCheckIn', @nuevo0 UNION ALL
    SELECT N'dbo.sp_Pago_ObtenerComprobante', @nuevo1 UNION ALL
    SELECT N'dbo.sp_Reporte_ServiciosMasUtilizados', @nuevo2
), objetos AS (
    SELECT nombre, esperada, OBJECT_ID(nombre, N'P') AS id FROM contratos
), definiciones AS (
    SELECT nombre, esperada, id, OBJECT_DEFINITION(id) AS observada FROM objetos
), posiciones AS (
    SELECT *, NULLIF(CHARINDEX(N'PROCEDURE', observada COLLATE Latin1_General_100_BIN2), 0) AS pos
    FROM definiciones
), cabeceras AS (
    SELECT *, REPLACE(REPLACE(REPLACE(SUBSTRING(observada COLLATE Latin1_General_100_BIN2, 1, pos - 1),
                  NCHAR(9), N' '), NCHAR(13), N' '), NCHAR(10), N' ') AS verbo
    FROM posiciones
), cuerpos AS (
    SELECT nombre, id, esperada, observada,
        SUBSTRING(esperada, LEN(N'CREATE OR ALTER PROCEDURE') + 1, DATALENGTH(esperada)) AS cuerpoEsperado,
        CASE WHEN UNICODE(RIGHT(verbo, 1)) = 32
                  AND RTRIM(verbo) COLLATE Latin1_General_100_BIN2 IN (N'CREATE', N'ALTER', N'CREATE OR ALTER')
             THEN SUBSTRING(observada, pos + LEN(N'PROCEDURE'), DATALENGTH(observada)) END AS cuerpoObservado
    FROM cabeceras
), comparaciones AS (
    SELECT *, CASE WHEN DATALENGTH(cuerpoEsperado) = DATALENGTH(cuerpoObservado)
                    AND cuerpoEsperado COLLATE Latin1_General_100_BIN2 = cuerpoObservado
                   THEN 1 ELSE 0 END AS coincide
    FROM cuerpos
)
-- FIN comparación del paso 10
SELECT TOP (1) @error = nombre + N': Definicion de consulta diferente a la revisada o no disponible.'
FROM comparaciones WHERE coincide = 0 ORDER BY nombre;
IF @error IS NOT NULL THROW 51905, @error, 1;
IF NOT EXISTS(SELECT 1 FROM sys.columns c JOIN sys.default_constraints d ON d.object_id=c.default_object_id
 WHERE c.object_id=OBJECT_ID(N'dbo.DetalleServicios') AND c.name=N'fecha_consumo'
 AND c.system_type_id IN (42,61) AND d.definition IN (N'(sysdatetime())',N'(getdate())',N'(SYSDATETIME())',N'(GETDATE())'))
 THROW 51906, 'Fecha de consumo incompatible o sin default conocido.', 1;
SELECT c.name,c.is_nullable,d.definition AS valor_nuevas_filas
FROM sys.columns c LEFT JOIN sys.default_constraints d ON d.object_id=c.default_object_id
WHERE c.object_id=OBJECT_ID(N'dbo.DetalleServicios') AND c.name=N'fecha_consumo';
SELECT N'Consultas verificadas. Revisar fecha historica si la columna ya existia; no se regularizan datos.' AS resultado;
