-- SOLO LECTURA: ejecutar con la identidad real de la aplicación, no con DBA.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51700, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SELECT ORIGINAL_LOGIN() AS login_original,USER_NAME() AS usuario_base,
 IS_SRVROLEMEMBER('sysadmin') AS es_sysadmin,IS_MEMBER('db_owner') AS es_db_owner;
SELECT N'sp_Huesped_HistorialReservas' AS procedimiento,
 HAS_PERMS_BY_NAME(N'dbo.sp_Huesped_HistorialReservas','OBJECT','EXECUTE') AS puede_ejecutar
UNION ALL
SELECT N'sp_ReservaTemporal_ConsultarConfirmacion',
 HAS_PERMS_BY_NAME(N'dbo.sp_ReservaTemporal_ConsultarConfirmacion','OBJECT','EXECUTE');
-- Ambos permisos deben ser 1; NULL no es aprobación.
-- Los DENY de DML de habitaciones/tipos deben continuar en 0 para la identidad operativa.
SELECT N'Habitaciones' AS tabla,
 HAS_PERMS_BY_NAME(N'dbo.Habitaciones','OBJECT','INSERT') AS insercion_directa,
 HAS_PERMS_BY_NAME(N'dbo.Habitaciones','OBJECT','UPDATE') AS modificacion_directa,
 HAS_PERMS_BY_NAME(N'dbo.Habitaciones','OBJECT','DELETE') AS borrado_directo
UNION ALL
SELECT N'TiposHabitacion',
 HAS_PERMS_BY_NAME(N'dbo.TiposHabitacion','OBJECT','INSERT'),
 HAS_PERMS_BY_NAME(N'dbo.TiposHabitacion','OBJECT','UPDATE'),
 HAS_PERMS_BY_NAME(N'dbo.TiposHabitacion','OBJECT','DELETE');
