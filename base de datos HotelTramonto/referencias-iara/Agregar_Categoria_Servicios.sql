
USE HotelTramontoDB;
GO

-- Agregar categoría sin perder los registros existentes
ALTER TABLE dbo.Servicios
ADD categoria VARCHAR(30) NOT NULL
    CONSTRAINT DF_Servicios_Categoria DEFAULT ('Otros');
GO

-- Clasificar los servicios que ya existen
UPDATE dbo.Servicios
SET categoria = 'Incluido'
WHERE nombre = 'Desayuno';

UPDATE dbo.Servicios
SET categoria = 'Otros'
WHERE nombre IN ('Lavanderia', 'Lavandería', 'Estacionamiento');
GO

-- Verificar los resultados
SELECT
    id_servicio,
    nombre,
    descripcion,
    categoria,
    precio,
    activo
FROM dbo.Servicios
ORDER BY categoria, nombre;
GO
