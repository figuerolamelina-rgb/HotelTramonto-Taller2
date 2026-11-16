
-- INSERTAR SERVICIO
CREATE OR ALTER PROCEDURE dbo.sp_Servicio_Insertar
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2),
    @categoria VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Servicios
        (nombre, descripcion, precio, categoria)
    VALUES
        (@nombre, @descripcion, @precio, @categoria);
END;
GO

-- MODIFICAR SERVICIO
CREATE OR ALTER PROCEDURE dbo.sp_Servicio_Modificar
    @id_servicio INT,
    @nombre VARCHAR(50),
    @descripcion VARCHAR(150) = NULL,
    @precio DECIMAL(10,2),
    @categoria VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Servicios
    SET
        nombre = @nombre,
        descripcion = @descripcion,
        precio = @precio,
        categoria = @categoria
    WHERE id_servicio = @id_servicio;
END;
GO

-- LISTAR SERVICIOS ACTIVOS
CREATE OR ALTER PROCEDURE dbo.sp_Servicio_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id_servicio,
        nombre,
        descripcion,
        categoria,
        precio,
        activo
    FROM dbo.Servicios
    WHERE activo = 1
    ORDER BY categoria, nombre;
END;
GO
