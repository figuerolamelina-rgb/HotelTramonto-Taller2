-- Complemento aditivo para revisión DBA. No se ejecuta por ningún runner.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51700, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51701, 'Ejecutar sin transaccion externa.', 1;
IF NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name=N'ht_app_compartida'
 AND type='R' AND is_fixed_role=0)
 THROW 51702, 'Falta el rol operativo aprobado; este script no crea roles ni miembros.', 1;
IF OBJECT_ID(N'dbo.sp_Huesped_HistorialReservas',N'P') IS NULL
 OR OBJECT_ID(N'dbo.sp_ReservaTemporal_ConsultarConfirmacion',N'P') IS NULL
 THROW 51703, 'Instalar primero los procedimientos de historial y recuperacion.', 1;
IF EXISTS(SELECT 1 FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
 WHERE o.object_id IN (OBJECT_ID(N'dbo.sp_Huesped_HistorialReservas'),
 OBJECT_ID(N'dbo.sp_ReservaTemporal_ConsultarConfirmacion'))
 AND COALESCE(o.principal_id,s.principal_id)<>DATABASE_PRINCIPAL_ID(N'dbo'))
 THROW 51704, 'Propietarios no compatibles con la cadena dbo.', 1;
IF EXISTS(SELECT 1 FROM sys.database_permissions WHERE grantee_principal_id=DATABASE_PRINCIPAL_ID(N'ht_app_compartida')
 AND state='D' AND permission_name IN ('EXECUTE','CONTROL') AND
 (class=0 OR (class=3 AND major_id=SCHEMA_ID(N'dbo')) OR
 (class=1 AND major_id IN (OBJECT_ID(N'dbo.sp_Huesped_HistorialReservas'),
 OBJECT_ID(N'dbo.sp_ReservaTemporal_ConsultarConfirmacion')))))
 THROW 51705, 'Hay DENY efectivo posible; revisar sin revocarlo automaticamente.', 1;
BEGIN TRY
 BEGIN TRANSACTION;
 GRANT EXECUTE ON OBJECT::dbo.sp_Huesped_HistorialReservas TO [ht_app_compartida];
 GRANT EXECUTE ON OBJECT::dbo.sp_ReservaTemporal_ConsultarConfirmacion TO [ht_app_compartida];
 COMMIT;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK;
 THROW;
END CATCH;
-- No sustituye la comprobación del token Windows real (grupos, DENY y otros roles).
