ALTER TABLE dbo.DetalleServicios
ADD fecha_consumo DATETIME2(0) NOT NULL
    CONSTRAINT DF_DetalleServicios_FechaConsumo
    DEFAULT (SYSDATETIME());