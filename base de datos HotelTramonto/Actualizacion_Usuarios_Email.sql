
USE HotelTramontoDB;
GO

-- Corrección 3: email obligatorio y único

IF EXISTS (
    SELECT 1
    FROM dbo.Usuarios
    WHERE email IS NULL
       OR LTRIM(RTRIM(email)) = ''
)
BEGIN
    THROW 50030, 'Existen usuarios sin email.', 1;
END;
GO

IF EXISTS (
    SELECT email
    FROM dbo.Usuarios
    GROUP BY email
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 50031, 'Existen emails duplicados.', 1;
END;
GO

-- Hacer obligatorio el email
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Usuarios')
      AND name = 'email'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.Usuarios
    ALTER COLUMN email VARCHAR(100) NOT NULL;
END;
GO

-- Agregar restricción de unicidad
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.Usuarios')
      AND name = 'UQ_Usuarios_Email'
)
BEGIN
    ALTER TABLE dbo.Usuarios
    ADD CONSTRAINT UQ_Usuarios_Email UNIQUE (email);
END;
GO
