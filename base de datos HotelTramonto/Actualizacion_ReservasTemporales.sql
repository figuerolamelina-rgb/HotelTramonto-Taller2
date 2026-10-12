
USE HotelTramontoDB;
GO

-- Agregar la modalidad de pago
IF COL_LENGTH('dbo.ReservasTemporales', 'modalidad_pago') IS NULL
BEGIN
    ALTER TABLE dbo.ReservasTemporales
    ADD modalidad_pago VARCHAR(20) NULL;
END;
GO

-- Comprobar que se agregó la columna
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ReservasTemporales'
  AND COLUMN_NAME = 'modalidad_pago';
GO



IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_ReservasTemporales_ModalidadPago'
      AND parent_object_id = OBJECT_ID('dbo.ReservasTemporales')
)
BEGIN
    ALTER TABLE dbo.ReservasTemporales
    ADD CONSTRAINT CK_ReservasTemporales_ModalidadPago
    CHECK (modalidad_pago IN ('PAGO_TOTAL', 'SENA_50'));
END;
GO

-- Comprobar que se creó la restricción
SELECT
    name AS Restriccion,
    definition AS Condicion
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.ReservasTemporales')
  AND name = 'CK_ReservasTemporales_ModalidadPago';
GO


-- Agregar modalidad de pago a Reservas
USE HotelTramontoDB;
GO

IF COL_LENGTH('dbo.Reservas', 'modalidad_pago') IS NULL
BEGIN
    ALTER TABLE dbo.Reservas
    ADD modalidad_pago VARCHAR(20) NULL;
END;
GO

-- Comprobar que la columna existe
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'Reservas'
  AND COLUMN_NAME = 'modalidad_pago';
GO




IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Reservas_ModalidadPago'
      AND parent_object_id = OBJECT_ID('dbo.Reservas')
)
BEGIN
    ALTER TABLE dbo.Reservas
    ADD CONSTRAINT CK_Reservas_ModalidadPago
    CHECK (modalidad_pago IN ('PAGO_TOTAL', 'SENA_50'));
END;
GO

SELECT
    name AS Restriccion,
    definition AS Condicion
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.Reservas')
  AND name = 'CK_Reservas_ModalidadPago';
GO



-- Corrección de una reserva temporal histórica
-- El pago inicial aprobado fue del 50 %

UPDATE dbo.ReservasTemporales
SET modalidad_pago = 'SENA_50'
WHERE id_reserva_temporal = 2
  AND id_reserva = 2
  AND estadoPago = 'APROBADO'
  AND modalidad_pago IS NULL
  AND EXISTS (
      SELECT 1
      FROM dbo.Pagos
      WHERE id_reserva = 2
        AND tipo_pago = 'PAGO_INICIAL'
        AND estado = 'APROBADO'
        AND monto = 50000
  );
GO

-- Agregar estado propio de la reserva temporal
IF COL_LENGTH('dbo.ReservasTemporales', 'estado') IS NULL
BEGIN
    ALTER TABLE dbo.ReservasTemporales
    ADD estado VARCHAR(20) NULL;
END;
GO

-- Comprobar que se agregó
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME = 'ReservasTemporales'
  AND COLUMN_NAME = 'estado';
GO


-- Completar el estado de las reservas anteriores
UPDATE dbo.ReservasTemporales
SET estado =
    CASE
        WHEN id_reserva IS NOT NULL
            THEN 'CONFIRMADA'
        WHEN fecha_expiracion <= GETDATE()
            THEN 'EXPIRADA'
        ELSE 'PENDIENTE'
    END
WHERE estado IS NULL;
GO

-- Comprobar los resultados
SELECT
    id_reserva_temporal,
    estadoPago,
    estado,
    id_reserva
FROM dbo.ReservasTemporales;
GO




IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_ReservasTemporales_Estado'
      AND parent_object_id = OBJECT_ID('dbo.ReservasTemporales')
)
BEGIN
    ALTER TABLE dbo.ReservasTemporales
    ADD CONSTRAINT CK_ReservasTemporales_Estado
    CHECK (
        estado IN (
            'PENDIENTE',
            'CONFIRMADA',
            'CANCELADA',
            'EXPIRADA'
        )
    );
END;
GO

-- Verificar la restriccion
SELECT
    name AS Restriccion,
    definition AS Condicion
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.ReservasTemporales')
  AND name = 'CK_ReservasTemporales_Estado';
GO
