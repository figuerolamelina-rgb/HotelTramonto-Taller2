
-- Listar servicios inactivos
CREATE OR ALTER PROCEDURE dbo.sp_Servicio_ListarInactivos
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
    WHERE activo = 0
    ORDER BY categoria, nombre;
END;
GO

-- Reactivar un servicio
CREATE OR ALTER PROCEDURE dbo.sp_Servicio_Reactivar
    @id_servicio INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Servicios
    SET activo = 1
    WHERE id_servicio = @id_servicio;
END;
GO
