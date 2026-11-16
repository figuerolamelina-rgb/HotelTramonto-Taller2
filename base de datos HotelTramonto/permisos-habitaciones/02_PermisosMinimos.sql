-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- PENDIENTE: fuera del recorrido actual de pruebas. No habilitar sin autorización posterior.
THROW 51201, 'Instalación de permisos pendiente: no ejecutar en esta etapa.', 1;
-- IMPLEMENTACIÓN MANUAL: revisar diagnóstico y definiciones antes de ejecutar.
-- NO distingue roles de empleados. Es un único rol técnico para la conexión Windows compartida.
-- Ejecutar como DBA en una base de pruebas. No usar la identidad operativa como DBA.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51020, 'Ejecutar fuera de una transacción existente.', 1;
DECLARE @UsuarioBase sysname = N'REEMPLAZAR_USUARIO_BASE'; -- usuario ya existente; no una contraseña
DECLARE @Rol sysname = N'ht_app_compartida';
IF @UsuarioBase = N'REEMPLAZAR_USUARIO_BASE' OR DATABASE_PRINCIPAL_ID(@UsuarioBase) IS NULL
    THROW 51010, 'Elegir un usuario de base existente y dedicado para la aplicación.', 1;
DECLARE @Id int = DATABASE_PRINCIPAL_ID(@UsuarioBase);
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE principal_id = @Id AND type NOT IN ('U','G'))
    THROW 51011, 'Se requiere un principal Windows existente.', 1;
-- No remover membresías automáticamente. El DBA debe resolverlas con el diagnóstico.
IF @Id = 1 OR EXISTS (
    SELECT 1 FROM sys.database_role_members rm
    JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
    WHERE rm.member_principal_id = @Id AND r.name <> @Rol)
    THROW 51012, 'Usar un usuario sin otros roles de base (incluido db_owner); revisar grupos Windows.', 1;
DECLARE @Login sysname = SUSER_SNAME((SELECT sid FROM sys.database_principals WHERE principal_id = @Id));
IF IS_SRVROLEMEMBER('sysadmin', @Login) = 1
    OR IS_SRVROLEMEMBER('securityadmin', @Login) = 1
    OR IS_SRVROLEMEMBER('serveradmin', @Login) = 1
    THROW 51013, 'La identidad tiene privilegios de servidor incompatibles con estas restricciones.', 1;
IF EXISTS (SELECT 1 FROM sys.schemas WHERE principal_id = @Id)
    OR EXISTS (SELECT 1 FROM sys.objects WHERE principal_id = @Id)
    OR EXISTS (SELECT 1 FROM sys.database_permissions WHERE grantee_principal_id = @Id
               AND state IN ('G','W') AND permission_name <> 'CONNECT')
    THROW 51014, 'La identidad posee objetos o permisos previos; revisar antes de aplicar.', 1;
-- Public y grupos Windows también pueden aportar privilegios. Comprobar con 00 y 03
-- bajo la identidad REAL. Este preflight no sustituye la inspección del token efectivo.
DECLARE @Procedimientos table (nombre sysname PRIMARY KEY);
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
-- No omitir silenciosamente funcionalidades existentes.
IF EXISTS (SELECT 1 FROM @Procedimientos WHERE OBJECT_ID(N'dbo.' + nombre, N'P') IS NULL)
BEGIN
    SELECT nombre AS procedimiento_faltante FROM @Procedimientos
    WHERE OBJECT_ID(N'dbo.' + nombre, N'P') IS NULL;
    THROW 51015, 'Faltan procedimientos requeridos por Datos. No se aplicaron permisos.', 1;
END;
-- Las cadenas de propiedad permiten operar mediante procedimientos sin dar DML directo.
IF EXISTS (SELECT 1 FROM sys.objects o JOIN sys.schemas s ON s.schema_id = o.schema_id
           WHERE s.name = N'dbo'
           AND ((o.type = 'U' AND o.name IN (N'Habitaciones',N'TiposHabitacion'))
                OR (o.type = 'P' AND EXISTS (SELECT 1 FROM @Procedimientos p WHERE p.nombre = o.name)))
           AND COALESCE(o.principal_id, s.principal_id) <> DATABASE_PRINCIPAL_ID(N'dbo'))
    THROW 51016, 'Revisar propietarios: la cadena de propiedad esperada no se cumple.', 1;
-- Instalación nueva solamente: no sobrescribir permisos ni membresías anteriores.
IF DATABASE_PRINCIPAL_ID(@Rol) IS NOT NULL
    THROW 51017, 'El rol ya existe. Revisar/revertir la instalación anterior; no se modifica.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Objeto sysname, @Sql nvarchar(max);
    IF DATABASE_PRINCIPAL_ID(@Rol) IS NULL
    BEGIN
        SET @Sql = N'CREATE ROLE ' + QUOTENAME(@Rol) + N' AUTHORIZATION dbo;';
        EXEC sys.sp_executesql @Sql;
    END;
    -- Si se reutiliza el rol, inspeccionar primero sus permisos: no se revocan permisos ajenos.
    IF EXISTS (SELECT 1 FROM sys.database_role_members WHERE member_principal_id = DATABASE_PRINCIPAL_ID(@Rol))
        THROW 51018, 'El rol técnico hereda otro rol; revisar antes de continuar.', 1;
    DECLARE tablas CURSOR LOCAL FAST_FORWARD FOR
        SELECT nombre FROM (VALUES (N'Habitaciones'),(N'TiposHabitacion')) t(nombre);
    OPEN tablas;
    FETCH NEXT FROM tablas INTO @Objeto;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'DENY INSERT, UPDATE, DELETE ON OBJECT::dbo.'
            + QUOTENAME(@Objeto) + N' TO ' + QUOTENAME(@Rol) + N';';
        EXEC sys.sp_executesql @Sql;
        FETCH NEXT FROM tablas INTO @Objeto;
    END;
    CLOSE tablas;
    DEALLOCATE tablas;
    DECLARE procedimientos CURSOR LOCAL FAST_FORWARD FOR SELECT nombre FROM @Procedimientos;
    OPEN procedimientos;
    FETCH NEXT FROM procedimientos INTO @Objeto;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @Sql = N'GRANT EXECUTE ON OBJECT::dbo.' + QUOTENAME(@Objeto) + N' TO ' + QUOTENAME(@Rol) + N';';
        EXEC sys.sp_executesql @Sql;
        FETCH NEXT FROM procedimientos INTO @Objeto;
    END;
    CLOSE procedimientos;
    DEALLOCATE procedimientos;
    IF NOT EXISTS (SELECT 1 FROM sys.database_role_members
                   WHERE role_principal_id = DATABASE_PRINCIPAL_ID(@Rol) AND member_principal_id = @Id)
    BEGIN
        SET @Sql = N'ALTER ROLE ' + QUOTENAME(@Rol) + N' ADD MEMBER ' + QUOTENAME(@UsuarioBase) + N';';
        EXEC sys.sp_executesql @Sql;
    END;
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
-- ALCANCE: solo DML directo de Habitaciones/TiposHabitacion. Sin DENY SELECT,
-- sin restricciones sobre tablas de huéspedes, reservas, pagos, servicios o usuarios.
-- En tablas restantes no se otorgan permisos directos; revisar grupos/public existentes.
-- NO se ejecuta ningún procedimiento de negocio durante esta instalación.
-- No aplicar DENY EXECUTE de administración a este rol: también lo usa ADMINISTRADOR.
