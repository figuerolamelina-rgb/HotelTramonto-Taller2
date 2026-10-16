
USE HotelTramontoDB;
GO

-- Corrección 4: usuario responsable obligatorio en reservas

IF EXISTS (
    SELECT 1
    FROM dbo.Reservas
    WHERE id_usuario IS NULL
)
BEGIN
    THROW 50040,
        'Existen reservas sin usuario responsable.',
        1;
END;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Quitar temporalmente el índice si existe
    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.Reservas')
          AND name = 'IX_Reservas_IdUsuario'
    )
    BEGIN
        DROP INDEX IX_Reservas_IdUsuario
        ON dbo.Reservas;
    END;

    -- Hacer obligatorio id_usuario si todavía permite NULL
    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.Reservas')
          AND name = 'id_usuario'
          AND is_nullable = 1
    )
    BEGIN
        ALTER TABLE dbo.Reservas
        ALTER COLUMN id_usuario INT NOT NULL;
    END;

    -- Restaurar el índice si no existe
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.Reservas')
          AND name = 'IX_Reservas_IdUsuario'
    )
    BEGIN
        CREATE INDEX IX_Reservas_IdUsuario
        ON dbo.Reservas(id_usuario);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
