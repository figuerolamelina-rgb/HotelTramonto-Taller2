IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
    THROW 51300, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51301, 'Ejecutar sin transaccion externa.', 1;

IF COL_LENGTH('dbo.DetalleServicios','id_habitacion') IS NULL
 OR COL_LENGTH('dbo.DetalleServicios','id_operacion') IS NULL
 THROW 51320, 'Falta migracion.', 1;
EXEC sys.sp_executesql N'
IF EXISTS (SELECT 1 FROM dbo.DetalleServicios ds WHERE ds.id_habitacion IS NOT NULL
 AND NOT EXISTS (SELECT 1 FROM dbo.DetalleReservas dr
 WHERE dr.id_reserva=ds.id_reserva AND dr.id_habitacion=ds.id_habitacion))
 THROW 51321, ''Asociacion inconsistente.'', 1;
SELECT id_reserva, COUNT(*) AS consumos,
 SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario) AS importe FROM dbo.DetalleServicios GROUP BY id_reserva;';
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.DetalleServicios')
 AND name='FK_DetalleServicios_ReservaHabitacion' AND is_disabled=0 AND is_not_trusted=0)
 THROW 51322, 'La FK falta o no es confiable.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.DetalleServicios')
 AND name='UX_DetalleServicios_Operacion' AND is_unique=1 AND is_disabled=0 AND has_filter=1)
 THROW 51323, 'Falta indice unico operativo.', 1;
SELECT name,is_disabled,is_not_trusted FROM sys.foreign_keys
 WHERE parent_object_id=OBJECT_ID('dbo.DetalleServicios');
SELECT name,is_unique,has_filter,filter_definition FROM sys.indexes
 WHERE object_id=OBJECT_ID('dbo.DetalleServicios');
