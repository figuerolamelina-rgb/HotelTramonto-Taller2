-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- SOLO LECTURA: no invoca procedimientos de escritura ni cambia datos.
SELECT COL_LENGTH(N'dbo.Servicios',N'categoria') AS longitud_categoria;
SELECT id_servicio,nombre,activo,categoria FROM dbo.Servicios
WHERE categoria IS NULL OR categoria COLLATE Latin1_General_100_BIN2 NOT IN
('Almuerzo','Merienda','Cena','Tragos','Pesca guiada','Incluido','Otros')
OR DATALENGTH(categoria)<>DATALENGTH(RTRIM(categoria));
SELECT categoria,activo,COUNT(*) AS cantidad FROM dbo.Servicios GROUP BY categoria,activo;
SELECT OBJECT_NAME(object_id) AS procedimiento,name,TYPE_NAME(user_type_id) AS tipo,max_length
FROM sys.parameters WHERE object_id IN
(OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'));
SELECT name,OBJECT_DEFINITION(object_id) AS definicion FROM sys.procedures
WHERE schema_id=SCHEMA_ID(N'dbo') AND name IN
('sp_Servicio_Insertar','sp_Servicio_Modificar','sp_Servicio_Listar',
 'sp_Servicio_ListarInactivos','sp_Servicio_Reactivar',
 'sp_Habitacion_ListarInactivas','sp_Habitacion_Reactivar','sp_Usuario_Reactivar');
-- Metadatos de resultados, sin ejecutar el procedimiento:
SELECT name,system_type_name,is_nullable,error_message
FROM sys.dm_exec_describe_first_result_set(N'EXEC dbo.sp_Servicio_Listar',NULL,0);
SELECT name,system_type_name,is_nullable,error_message
FROM sys.dm_exec_describe_first_result_set(N'EXEC dbo.sp_Servicio_ListarInactivos',NULL,0);

GO
-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- Revisión de cargos históricos asociados a servicios ahora clasificados Incluido.
-- No son automáticamente cargos erróneos: la clasificación pudo cambiar con el tiempo.
-- No elimina consumos, recalcula saldos ni genera devoluciones.
SELECT d.id_detalle_servicio,d.id_reserva,s.id_servicio,s.nombre,
       d.cantidad,d.precio_unitario,d.cantidad*d.precio_unitario AS subtotal
FROM dbo.DetalleServicios d JOIN dbo.Servicios s ON s.id_servicio=d.id_servicios
WHERE s.categoria='Incluido';
