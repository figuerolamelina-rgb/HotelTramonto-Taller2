-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- REVISAR ANTES DE EJECUTAR. No altera definiciones existentes ni datos al instalar.
-- Crear en una base de pruebas primero. Los originales se conservan intactos.
SET XACT_ABORT ON;
-- El esquema real debe coincidir con C#. No se crean columnas ni categorías automáticamente.
IF @@TRANCOUNT <> 0 THROW 51000, 'Ejecutar fuera de una transacción existente.', 1;
DECLARE @Columnas table (tabla sysname, columna sysname);
INSERT INTO @Columnas VALUES
('Habitaciones','id_habitacion'),('Habitaciones','tipo_habitacion'),('Habitaciones','piso'),
('Habitaciones','numero'),('Habitaciones','estado'),('Habitaciones','activo'),
('TiposHabitacion','id_tipoHabitacion'),('TiposHabitacion','nombre'),
('Servicios','id_servicio'),('Servicios','nombre'),('Servicios','descripcion'),
('Servicios','categoria'),('Servicios','precio'),('Servicios','activo'),
('Usuarios','id_usuario'),('Usuarios','activo');
IF EXISTS (SELECT 1 FROM @Columnas WHERE COL_LENGTH(N'dbo.' + tabla, columna) IS NULL)
BEGIN
    SELECT tabla, columna AS columna_faltante FROM @Columnas
    WHERE COL_LENGTH(N'dbo.' + tabla, columna) IS NULL;
    THROW 51003, 'Esquema incompatible. Revisar columnas; Servicios.categoria es requerida por C#. No se instala nada.', 1;
END;
BEGIN TRY
    BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.sp_Habitacion_ListarInactivas') IS NOT NULL AND OBJECT_ID(N'dbo.sp_Habitacion_ListarInactivas', N'P') IS NULL
    THROW 51001, 'Existe un objeto incompatible con el procedimiento previsto.', 1;
IF OBJECT_ID(N'dbo.sp_Habitacion_ListarInactivas', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.sp_Habitacion_ListarInactivas
AS
BEGIN
    SET NOCOUNT ON;
    SELECT H.id_habitacion, H.tipo_habitacion, T.nombre AS tipo,
           H.piso, H.numero, H.estado, H.activo
    FROM dbo.Habitaciones H
    INNER JOIN dbo.TiposHabitacion T ON T.id_tipoHabitacion = H.tipo_habitacion
    WHERE H.activo = 0
    ORDER BY H.numero;
END');
ELSE
    PRINT N'sp_Habitacion_ListarInactivas: existente, no se modifica; comparar su definición.';
IF OBJECT_ID(N'dbo.sp_Habitacion_Reactivar') IS NOT NULL AND OBJECT_ID(N'dbo.sp_Habitacion_Reactivar', N'P') IS NULL
    THROW 51001, 'Existe un objeto incompatible con el procedimiento previsto.', 1;
IF OBJECT_ID(N'dbo.sp_Habitacion_Reactivar', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.sp_Habitacion_Reactivar
    @id_habitacion INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Habitaciones WHERE id_habitacion = @id_habitacion)
        THROW 51002, ''La habitación no existe.'', 1;
    -- Reactivar no implica cambiar manualmente el estado ni modificar reservas.
    UPDATE dbo.Habitaciones SET activo = 1 WHERE id_habitacion = @id_habitacion;
END');
ELSE
    PRINT N'sp_Habitacion_Reactivar: existente, no se modifica; comparar su definición.';
IF OBJECT_ID(N'dbo.sp_Servicio_ListarInactivos') IS NOT NULL AND OBJECT_ID(N'dbo.sp_Servicio_ListarInactivos', N'P') IS NULL
    THROW 51001, 'Existe un objeto incompatible con el procedimiento previsto.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_ListarInactivos', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.sp_Servicio_ListarInactivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id_servicio, nombre, descripcion, categoria, precio, activo
    FROM dbo.Servicios WHERE activo = 0 ORDER BY nombre;
END');
ELSE
    PRINT N'sp_Servicio_ListarInactivos: existente, no se modifica; comparar su definición.';
IF OBJECT_ID(N'dbo.sp_Servicio_Reactivar') IS NOT NULL AND OBJECT_ID(N'dbo.sp_Servicio_Reactivar', N'P') IS NULL
    THROW 51001, 'Existe un objeto incompatible con el procedimiento previsto.', 1;
IF OBJECT_ID(N'dbo.sp_Servicio_Reactivar', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.sp_Servicio_Reactivar
    @id_servicio INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @id_servicio IS NULL OR @id_servicio <= 0
        THROW 51004, ''El servicio no es válido.'', 1;
    UPDATE dbo.Servicios SET activo = 1 WHERE id_servicio = @id_servicio;
    IF @@ROWCOUNT = 0 THROW 51005, ''El servicio no existe.'', 1;
END');
ELSE
    PRINT N'sp_Servicio_Reactivar: existente, no se modifica; comparar su definición.';
IF OBJECT_ID(N'dbo.sp_Usuario_Reactivar') IS NOT NULL AND OBJECT_ID(N'dbo.sp_Usuario_Reactivar', N'P') IS NULL
    THROW 51001, 'Existe un objeto incompatible con el procedimiento previsto.', 1;
IF OBJECT_ID(N'dbo.sp_Usuario_Reactivar', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.sp_Usuario_Reactivar
    @id_usuario INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @id_usuario IS NULL OR @id_usuario <= 0
        THROW 51006, ''El usuario no es válido.'', 1;
    UPDATE dbo.Usuarios SET activo = 1 WHERE id_usuario = @id_usuario;
    IF @@ROWCOUNT = 0 THROW 51007, ''El usuario no existe.'', 1;
END');
ELSE
    PRINT N'sp_Usuario_Reactivar: existente, no se modifica; comparar su definición.';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
