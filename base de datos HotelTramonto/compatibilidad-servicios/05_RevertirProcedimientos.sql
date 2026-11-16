-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- REVERSIÓN MANUAL NO DESTRUCTIVA. Basada en las definiciones reales confirmadas.
-- No usar si los procedimientos se modificaron después: comparar primero con respaldos.
-- Detener escrituras. Restaura el contrato ANTERIOR, incompatible con C# que envía categoria.
-- Conserva columna y clasificaciones; no borra servicios, consumos ni pagos.
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51140, 'Ejecutar sin transacción externa.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Insertar',N'P') IS NULL THROW 51141, 'Falta sp_Servicio_Insertar. No se recrea automáticamente.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Modificar',N'P') IS NULL THROW 51141, 'Falta sp_Servicio_Modificar. No se recrea automáticamente.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Listar',N'P') IS NULL THROW 51141, 'Falta sp_Servicio_Listar. No se recrea automáticamente.', 1;
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN
 (OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar')))
    THROW 51142, 'Hay firmas: revisar reversión con el DBA.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Insertar
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Servicios(nombre, descripcion, precio)
    VALUES(@nombre, @descripcion, @precio);
END');
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Modificar
    @id_servicio INT,
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Servicios
    SET nombre=@nombre, descripcion=@descripcion, precio=@precio
    WHERE id_servicio=@id_servicio;
END');
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_servicio, nombre, descripcion, precio, activo
    FROM dbo.Servicios WHERE activo=1 ORDER BY nombre;
END');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- No eliminar categoria: perdería las decisiones históricas. No revierte clasificaciones
-- ni datos de pruebas funcionales. Reaplicar 03 cuando se haya diagnosticado el problema.
