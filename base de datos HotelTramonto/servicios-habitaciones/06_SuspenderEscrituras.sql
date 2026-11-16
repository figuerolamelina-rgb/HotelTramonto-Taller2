IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51400, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51401, 'Ejecutar sin transaccion externa.', 1;
-- Recuperacion conservadora: detener clientes primero; conserva TODO dato y esquema.
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN
 (OBJECT_ID('dbo.sp_Pago_Insertar'),OBJECT_ID('dbo.sp_Pago_ActualizarEstado'),
 OBJECT_ID('dbo.sp_Pago_RegistrarServicios'),OBJECT_ID('dbo.sp_Pago_RegistrarSaldoAlojamiento'),
 OBJECT_ID('dbo.sp_DetalleServicio_Insertar'),OBJECT_ID('dbo.sp_Reserva_CheckOut')))
 THROW 51431, 'Modulos firmados: coordinar suspension con DBA.', 1;
BEGIN TRY
 BEGIN TRANSACTION;
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_Insertar @id_reserva INT=NULL,@id_reserva_temporal INT=NULL,@id_usuario INT,@monto DECIMAL(10,2),@metodo_pago VARCHAR(30),@estado VARCHAR(20),@tipo_pago VARCHAR(30),@id_operacion UNIQUEIDENTIFIER=NULL AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_ActualizarEstado @id_pago INT,@estado VARCHAR(20) AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarServicios @id_reserva INT,@id_usuario INT,@metodo_pago VARCHAR(30),@id_operacion UNIQUEIDENTIFIER=NULL AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarSaldoAlojamiento @id_reserva INT,@id_usuario INT,@metodo_pago VARCHAR(30),@id_operacion UNIQUEIDENTIFIER=NULL AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_DetalleServicio_Insertar @id_servicios INT,@id_reserva INT,@cantidad INT,@precio_unitario DECIMAL(10,2),@id_habitacion INT,@id_operacion UNIQUEIDENTIFIER AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckOut @id_reserva INT AS THROW 51435, ''Escrituras suspendidas por recuperacion. Contacte al responsable.'', 1;';
 COMMIT;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK;
 THROW;
END CATCH;
