-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- FINALIZADO con las tres definiciones reales aportadas por las integrantes.
-- Respaldar la copia de pruebas y guardar los ALTER actuales antes de ejecutar.
-- Conserva lógica, parámetros anteriores, filtro activo=1 y orden por nombre.
-- No invoca procedimientos ni modifica filas al instalar las definiciones.
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51120, 'Ejecutar sin transacción externa.', 1;
IF COL_LENGTH(N'dbo.Servicios',N'categoria') IS NULL THROW 51122, 'Falta categoria.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Insertar',N'P') IS NULL THROW 51123, 'Falta sp_Servicio_Insertar.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Modificar',N'P') IS NULL THROW 51123, 'Falta sp_Servicio_Modificar.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Listar',N'P') IS NULL THROW 51123, 'Falta sp_Servicio_Listar.', 1;
-- ALTER elimina firmas de módulos: no continuar automáticamente si hay alguna.
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN
 (OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar')))
    THROW 51125, 'Hay módulos firmados. Coordinar respaldo y reposición de firmas con el DBA.', 1;
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Servicios')
              AND name=N'categoria' AND system_type_id=167 AND max_length=30 AND is_nullable=1)
    THROW 51126, 'Se espera categoria VARCHAR(30) NULL. Revisar el esquema.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    -- HOLDLOCK evita activar entre validación e instalación con categorías pendientes.
    IF EXISTS(SELECT 1 FROM dbo.Servicios WITH(UPDLOCK,HOLDLOCK) WHERE categoria IS NULL
              OR categoria COLLATE Latin1_General_100_BIN2 NOT IN
              ('Almuerzo','Merienda','Cena','Tragos','Pesca guiada','Incluido','Otros')
              OR DATALENGTH(categoria)<>DATALENGTH(RTRIM(categoria)))
        THROW 51124, 'Clasificar todos los servicios antes de habilitar el contrato nuevo.', 1;
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Insertar
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2),
    @categoria VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF @categoria IS NULL OR @categoria COLLATE Latin1_General_100_BIN2 NOT IN
       (''Almuerzo'',''Merienda'',''Cena'',''Tragos'',''Pesca guiada'',''Incluido'',''Otros'')
       OR DATALENGTH(@categoria)<>DATALENGTH(RTRIM(@categoria))
        THROW 51130, ''Seleccioná una categoría válida.'', 1;
    INSERT INTO dbo.Servicios(nombre,descripcion,precio,categoria)
    VALUES(@nombre,@descripcion,@precio,@categoria);
END');
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Modificar
    @id_servicio INT,
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2),
    @categoria VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF @categoria IS NULL OR @categoria COLLATE Latin1_General_100_BIN2 NOT IN
       (''Almuerzo'',''Merienda'',''Cena'',''Tragos'',''Pesca guiada'',''Incluido'',''Otros'')
       OR DATALENGTH(@categoria)<>DATALENGTH(RTRIM(@categoria))
        THROW 51130, ''Seleccioná una categoría válida.'', 1;
    UPDATE dbo.Servicios
    SET nombre=@nombre,descripcion=@descripcion,precio=@precio,categoria=@categoria
    WHERE id_servicio=@id_servicio;
END');
    EXEC(N'ALTER PROCEDURE dbo.sp_Servicio_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_servicio,nombre,descripcion,categoria,precio,activo
    FROM dbo.Servicios WHERE activo=1 ORDER BY nombre;
END');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
