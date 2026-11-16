-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- ADITIVO E IDEMPOTENTE. Revisar antes de ejecutar en una copia de pruebas.
-- VARCHAR(30) cubre las siete categorías C#; NULL conserva la falta de clasificación histórica.
-- No hay DEFAULT, backfill automático, eliminación ni recreación de tablas.
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51100, 'Ejecutar sin una transacción externa.', 1;
IF OBJECT_ID(N'dbo.Servicios',N'U') IS NULL THROW 51101, 'Falta dbo.Servicios.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    IF COL_LENGTH(N'dbo.Servicios',N'categoria') IS NULL
        EXEC(N'ALTER TABLE dbo.Servicios ADD categoria VARCHAR(30) NULL;');
    ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Servicios')
                       AND name=N'categoria' AND system_type_id=167 AND max_length=30 AND is_nullable=1)
        THROW 51102, 'La columna existente no es VARCHAR(30) NULL. Revisar, no se altera automáticamente.', 1;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
