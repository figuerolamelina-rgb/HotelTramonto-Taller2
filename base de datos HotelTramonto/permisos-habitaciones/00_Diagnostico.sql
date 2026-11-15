-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- SOLO LECTURA. Ejecutar primero como la identidad que usa la aplicación.
SELECT ORIGINAL_LOGIN() AS login_original, SUSER_SNAME() AS contexto,
       USER_NAME() AS usuario_base, IS_SRVROLEMEMBER('sysadmin') AS es_sysadmin,
       IS_MEMBER('db_owner') AS es_db_owner;
-- sysadmin/dbo/db_owner pueden eludir o alterar la protección. No usarlos en la app.
SELECT * FROM sys.fn_my_permissions(NULL, 'DATABASE');
SELECT * FROM sys.fn_my_permissions('dbo.Habitaciones', 'OBJECT');
SELECT * FROM sys.fn_my_permissions('dbo.TiposHabitacion', 'OBJECT');
SELECT r.name AS rol, m.name AS miembro
FROM sys.database_role_members rm
JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
JOIN sys.database_principals m ON m.principal_id = rm.member_principal_id;
SELECT p.name AS principal, pe.state_desc, pe.permission_name, pe.class_desc,
       OBJECT_SCHEMA_NAME(pe.major_id) AS esquema, OBJECT_NAME(pe.major_id) AS objeto
FROM sys.database_permissions pe
JOIN sys.database_principals p ON p.principal_id = pe.grantee_principal_id;
SELECT s.name AS esquema, o.name, o.type_desc, USER_NAME(o.principal_id) AS propietario_explicito,
       USER_NAME(s.principal_id) AS propietario_esquema
FROM sys.objects o JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.is_ms_shipped = 0 AND o.type IN ('P','U','TR');
-- Una definición NULL puede significar falta de VIEW DEFINITION; revisar como DBA.
SELECT name, OBJECT_DEFINITION(object_id) AS definicion
FROM sys.procedures WHERE name LIKE 'sp_Habitacion_%' OR name LIKE 'sp_TipoHabitacion_%';

GO
-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- Contratos de los cinco procedimientos faltantes; no consulta datos de usuarios.
SELECT s.name AS esquema, t.name AS tabla, c.name AS columna,
       TYPE_NAME(c.user_type_id) AS tipo, c.max_length, c.is_nullable
FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
WHERE s.name = 'dbo' AND t.name IN ('Habitaciones','TiposHabitacion','Servicios','Usuarios')
ORDER BY t.name, c.column_id;
SELECT n.nombre, CASE WHEN OBJECT_ID(N'dbo.' + n.nombre, N'P') IS NULL
       THEN 'NO ENCONTRADO (o sin visibilidad)' ELSE 'EXISTENTE' END AS estado
FROM (VALUES (N'sp_Habitacion_Reactivar'), (N'sp_Habitacion_ListarInactivas'),
             (N'sp_Servicio_Reactivar'), (N'sp_Servicio_ListarInactivos'),
             (N'sp_Usuario_Reactivar')) n(nombre);
