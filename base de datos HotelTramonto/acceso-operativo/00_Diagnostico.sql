IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51800, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
IF COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')),N'')<>N'LAPTOP-5TSJJUP8'
 OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')),N'')<>N'SQLEXPRESS'
 THROW 51805, 'Instancia no autorizada.', 1;
-- Solo lectura. Ejecutar primero como DBA y después con la identidad operativa REAL.
SELECT ORIGINAL_LOGIN() AS login_original, SUSER_SNAME() AS login_actual, USER_NAME() AS usuario_base,
 IS_SRVROLEMEMBER('sysadmin') AS sysadmin, IS_MEMBER('db_owner') AS db_owner;
SELECT name,type,usage FROM sys.login_token;
SELECT name,type,usage FROM sys.user_token;
SELECT pr.name,pe.state_desc,pe.permission_name,pe.class_desc,OBJECT_NAME(pe.major_id) AS objeto,pe.minor_id
FROM sys.database_permissions pe JOIN sys.database_principals pr ON pr.principal_id=pe.grantee_principal_id;
SELECT r.name AS rol,m.name AS miembro FROM sys.database_role_members rm
 JOIN sys.database_principals r ON r.principal_id=rm.role_principal_id
 JOIN sys.database_principals m ON m.principal_id=rm.member_principal_id;
-- Revisar grupos Windows, permisos de columna, IMPERSONATE, public y propietarios.
