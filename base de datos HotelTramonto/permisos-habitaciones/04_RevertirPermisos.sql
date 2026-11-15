-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- REVISAR. Reversión de 02 únicamente; no ejecutar automáticamente.
-- 02 exige un rol nuevo: no existe estado previo del rol que restaurar.
-- Usar el MISMO usuario elegido al instalar. No cambiar roles del servidor ni db_owner.
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51030, 'Ejecutar fuera de una transacción existente.', 1;
DECLARE @UsuarioBase sysname = N'REEMPLAZAR_USUARIO_BASE';
DECLARE @Rol sysname = N'ht_app_compartida';
IF @UsuarioBase = N'REEMPLAZAR_USUARIO_BASE' OR DATABASE_PRINCIPAL_ID(@UsuarioBase) IS NULL
    THROW 51031, 'Indicar el usuario de base utilizado en la instalación.', 1;
IF DATABASE_PRINCIPAL_ID(@Rol) IS NULL
BEGIN
    PRINT N'Rol inexistente; no hay permisos de esta instalación que revertir.';
    RETURN;
END;
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @Rol AND type = 'R' AND is_fixed_role = 0)
    THROW 51032, 'Principal incompatible. Revisar manualmente.', 1;
IF EXISTS (SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID(@Rol)
           AND member_principal_id <> DATABASE_PRINCIPAL_ID(@UsuarioBase))
    OR EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = DATABASE_PRINCIPAL_ID(@Rol))
    OR EXISTS (SELECT 1 FROM sys.schemas WHERE principal_id = DATABASE_PRINCIPAL_ID(@Rol))
    OR EXISTS (SELECT 1 FROM sys.objects WHERE principal_id = DATABASE_PRINCIPAL_ID(@Rol))
    THROW 51033, 'El rol fue reutilizado, posee objetos o tiene otras membresías. Revertir manualmente.', 1;
DECLARE @Procedimientos table(nombre sysname PRIMARY KEY);
INSERT INTO @Procedimientos VALUES
(N'sp_ConfirmarReserva'),
(N'sp_DetalleReserva_Insertar'),
(N'sp_DetalleReserva_ListarPorReserva'),
(N'sp_DetalleReserva_Modificar'),
(N'sp_DetalleServicio_Insertar'),
(N'sp_DetalleServicio_ListarPorReserva'),
(N'sp_Habitacion_ConsultarDisponibilidad'),
(N'sp_Habitacion_Desactivar'),
(N'sp_Habitacion_Insertar'),
(N'sp_Habitacion_Listar'),
(N'sp_Habitacion_ListarInactivas'),
(N'sp_Habitacion_Modificar'),
(N'sp_Habitacion_Reactivar'),
(N'sp_Huesped_BuscarPorDNI'),
(N'sp_Huesped_Desactivar'),
(N'sp_Huesped_Insertar'),
(N'sp_Huesped_Listar'),
(N'sp_Huesped_Modificar'),
(N'sp_Huesped_Reactivar'),
(N'sp_Pago_ActualizarEstado'),
(N'sp_Pago_Insertar'),
(N'sp_Pago_ListarPorReserva'),
(N'sp_Pago_RegistrarSaldoAlojamiento'),
(N'sp_Pago_RegistrarServicios'),
(N'sp_Reporte_EstadisticasReservas'),
(N'sp_Reporte_HabitacionesMasUtilizadas'),
(N'sp_Reporte_Ingresos'),
(N'sp_Reporte_Ocupacion'),
(N'sp_Reporte_ServiciosMasUtilizados'),
(N'sp_ReservaTemporal_ActualizarEstado'),
(N'sp_ReservaTemporal_ExpirarVencidas'),
(N'sp_ReservaTemporal_Insertar'),
(N'sp_ReservaTemporal_ListarPendientes'),
(N'sp_Reserva_Cancelar'),
(N'sp_Reserva_CheckIn'),
(N'sp_Reserva_CheckOut'),
(N'sp_Reserva_ConsultarSaldo'),
(N'sp_Reserva_Listar'),
(N'sp_Reserva_ListarPorEstado'),
(N'sp_Reserva_ListarPorFechas'),
(N'sp_Reserva_ListarPorHuesped'),
(N'sp_Rol_Listar'),
(N'sp_Servicio_Desactivar'),
(N'sp_Servicio_Insertar'),
(N'sp_Servicio_Listar'),
(N'sp_Servicio_ListarInactivos'),
(N'sp_Servicio_Modificar'),
(N'sp_Servicio_Reactivar'),
(N'sp_TipoHabitacion_Desactivar'),
(N'sp_TipoHabitacion_Insertar'),
(N'sp_TipoHabitacion_Listar'),
(N'sp_TipoHabitacion_Modificar'),
(N'sp_Usuario_BuscarPorDNI'),
(N'sp_Usuario_BuscarPorEmail'),
(N'sp_Usuario_CambiarContrasena'),
(N'sp_Usuario_Desactivar'),
(N'sp_Usuario_Insertar'),
(N'sp_Usuario_Listar'),
(N'sp_Usuario_Modificar'),
(N'sp_Usuario_Reactivar');
IF EXISTS (
    SELECT 1 FROM sys.database_permissions pe
    WHERE pe.grantee_principal_id = DATABASE_PRINCIPAL_ID(@Rol)
      AND NOT (pe.class = 1 AND pe.minor_id = 0 AND (
        (pe.state = 'D' AND pe.permission_name IN ('INSERT','UPDATE','DELETE')
         AND pe.major_id IN (OBJECT_ID(N'dbo.Habitaciones'),OBJECT_ID(N'dbo.TiposHabitacion')))
        OR (pe.state = 'G' AND pe.permission_name = 'EXECUTE'
            AND EXISTS (SELECT 1 FROM @Procedimientos p WHERE OBJECT_ID(N'dbo.' + p.nombre, N'P') = pe.major_id)))))
    THROW 51034, 'El rol contiene permisos distintos de la instalación prevista. Revisar manualmente.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Sql nvarchar(max);
    IF EXISTS (SELECT 1 FROM sys.database_role_members
               WHERE role_principal_id = DATABASE_PRINCIPAL_ID(@Rol)
               AND member_principal_id = DATABASE_PRINCIPAL_ID(@UsuarioBase))
    BEGIN
        SET @Sql = N'ALTER ROLE ' + QUOTENAME(@Rol) + N' DROP MEMBER ' + QUOTENAME(@UsuarioBase) + N';';
        EXEC sys.sp_executesql @Sql;
    END;
    SET @Sql = N'DROP ROLE ' + QUOTENAME(@Rol) + N';';
    EXEC sys.sp_executesql @Sql;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- Elimina los GRANT/DENY del rol nuevo, NO elimina usuarios, tablas, datos o procedimientos.
-- No concede permisos alternativos: si antes el usuario no tenía acceso, volverá a no tenerlo.
