-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- SOLO LECTURA. Ejecutar primero y guardar resultados fuera de Git.
SELECT t.name AS tabla, c.column_id, c.name AS columna, ty.name AS tipo,
       c.max_length, c.precision, c.scale, c.is_nullable, c.collation_name,
       dc.definition AS valor_predeterminado
FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
WHERE s.name=N'dbo' AND t.name IN (N'Servicios',N'DetalleServicios')
ORDER BY t.name,c.column_id;
SELECT name, definition FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.Servicios');
SELECT i.name, i.is_unique, i.is_primary_key, c.name AS columna, ic.key_ordinal
FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
WHERE i.object_id=OBJECT_ID(N'dbo.Servicios');
SELECT name, is_disabled, OBJECT_DEFINITION(object_id) AS definicion
FROM sys.triggers WHERE parent_id=OBJECT_ID(N'dbo.Servicios');
-- Guardar además cada procedimiento con SSMS > Script as > ALTER To > File.
SELECT p.name, m.definition, m.uses_ansi_nulls, m.uses_quoted_identifier,
       m.execute_as_principal_id
FROM sys.procedures p LEFT JOIN sys.sql_modules m ON m.object_id=p.object_id
WHERE p.schema_id=SCHEMA_ID(N'dbo') AND p.name IN
(N'sp_Servicio_Insertar',N'sp_Servicio_Modificar',N'sp_Servicio_Listar');
SELECT OBJECT_NAME(object_id) AS procedimiento, name AS parametro,
       TYPE_NAME(user_type_id) AS tipo, max_length, precision, scale, is_output
FROM sys.parameters WHERE object_id IN (OBJECT_ID(N'dbo.sp_Servicio_Insertar'),
 OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar'));
SELECT USER_NAME(grantee_principal_id) AS principal, state_desc, permission_name,
       OBJECT_NAME(major_id) AS procedimiento
FROM sys.database_permissions WHERE class=1 AND major_id IN
(OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar'));
SELECT referencing_schema_name, referencing_entity_name
FROM sys.dm_sql_referencing_entities(N'dbo.Servicios',N'OBJECT');

GO
-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- No se imprimen hashes, contraseñas ni filas de usuarios.

-- Las firmas de módulos deben reponerse si se altera un procedimiento firmado.
SELECT OBJECT_NAME(major_id) AS modulo_firmado, crypt_type_desc
FROM sys.crypt_properties WHERE class=1 AND major_id IN
(OBJECT_ID(N'dbo.sp_Servicio_Insertar'),OBJECT_ID(N'dbo.sp_Servicio_Modificar'),OBJECT_ID(N'dbo.sp_Servicio_Listar'));
