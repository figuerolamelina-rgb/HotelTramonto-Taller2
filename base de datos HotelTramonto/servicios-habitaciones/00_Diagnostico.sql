IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
    THROW 51300, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51301, 'Ejecutar sin transaccion externa.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Servicios')
 AND name='categoria' AND system_type_id=167 AND max_length=30 AND is_nullable=1)
 THROW 51302, 'Instalar primero la compatibilidad Servicios.categoria de correccion 2.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.sp_Servicio_Insertar') AND name='@categoria')
 OR NOT EXISTS (SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.sp_Servicio_Modificar') AND name='@categoria')
 THROW 51303, 'Faltan procedimientos compatibles con categoria.', 1;
IF EXISTS (SELECT 1 FROM dbo.Servicios WHERE categoria IS NULL OR categoria NOT IN
 ('Almuerzo','Merienda','Cena','Tragos','Pesca guiada','Incluido','Otros'))
 THROW 51304, 'Resolver clasificacion de servicios antes de continuar.', 1;

-- Guardar resultados y respaldo antes de instalar. No modifica datos.
SELECT DB_NAME() AS base_actual, @@SERVERNAME AS instancia;
SELECT COUNT(*) AS consumos, SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario) AS importe
FROM dbo.DetalleServicios;
SELECT o.name, OBJECT_DEFINITION(o.object_id) AS definicion_previa
FROM sys.objects o WHERE o.name IN ('sp_DetalleServicio_Insertar','sp_DetalleServicio_ListarPorReserva','sp_Pago_RegistrarServicios','sp_Reserva_CheckOut','sp_Pago_Insertar','sp_Pago_ActualizarEstado','sp_Pago_RegistrarSaldoAlojamiento');
SELECT name, is_disabled, is_not_trusted FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.DetalleServicios');

SELECT id_pago,id_reserva,id_reserva_temporal,monto,estado,tipo_pago FROM dbo.Pagos ORDER BY id_pago;
-- Guardar resultados de forma privada para comprobar que la instalacion no modifica filas.
