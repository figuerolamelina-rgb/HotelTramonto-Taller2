IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51800, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
IF COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('MachineName')),N'')<>N'LAPTOP-5TSJJUP8'
 OR COALESCE(CONVERT(nvarchar(128),SERVERPROPERTY('InstanceName')),N'')<>N'SQLEXPRESS'
 THROW 51805, 'Instancia no autorizada.', 1;
-- Solo lectura, bajo identidad Windows operativa REAL, sin EXECUTE AS.
IF COALESCE(IS_SRVROLEMEMBER('sysadmin'),1)<>0 OR COALESCE(IS_MEMBER('db_owner'),1)<>0
 OR USER_NAME()=N'dbo'
 THROW 51801, 'Una identidad elevada o indeterminada no valida privilegios minimos.', 1;
SELECT ORIGINAL_LOGIN() AS identidad,USER_NAME() AS usuario_base;
DECLARE @Rutas TABLE(nombre sysname);
INSERT @Rutas VALUES
(N'dbo.sp_Huesped_HistorialReservas'),
(N'dbo.sp_ReservaTemporal_ConsultarConfirmacion'),
(N'dbo.sp_ConfirmarReserva'),
(N'dbo.sp_DetalleReserva_Insertar'),
(N'dbo.sp_DetalleReserva_ListarPorReserva'),
(N'dbo.sp_DetalleReserva_Modificar'),
(N'dbo.sp_DetalleServicio_Insertar'),
(N'dbo.sp_DetalleServicio_ListarPorReserva'),
(N'dbo.sp_Habitacion_ConsultarDisponibilidad'),
(N'dbo.sp_Habitacion_Desactivar'),
(N'dbo.sp_Habitacion_Insertar'),
(N'dbo.sp_Habitacion_Listar'),
(N'dbo.sp_Habitacion_ListarInactivas'),
(N'dbo.sp_Habitacion_Modificar'),
(N'dbo.sp_Habitacion_Reactivar'),
(N'dbo.sp_Huesped_BuscarPorDNI'),
(N'dbo.sp_Huesped_Desactivar'),
(N'dbo.sp_Huesped_Insertar'),
(N'dbo.sp_Huesped_Listar'),
(N'dbo.sp_Huesped_Modificar'),
(N'dbo.sp_Huesped_Reactivar'),
(N'dbo.sp_Pago_ActualizarEstado'),
(N'dbo.sp_Pago_Insertar'),
(N'dbo.sp_Pago_ListarPorReserva'),
(N'dbo.sp_Pago_RegistrarSaldoAlojamiento'),
(N'dbo.sp_Pago_RegistrarServicios'),
(N'dbo.sp_Reporte_EstadisticasReservas'),
(N'dbo.sp_Reporte_HabitacionesMasUtilizadas'),
(N'dbo.sp_Reporte_Ingresos'),
(N'dbo.sp_Reporte_Ocupacion'),
(N'dbo.sp_Reporte_ServiciosMasUtilizados'),
(N'dbo.sp_ReservaTemporal_ActualizarEstado'),
(N'dbo.sp_ReservaTemporal_ExpirarVencidas'),
(N'dbo.sp_ReservaTemporal_Insertar'),
(N'dbo.sp_ReservaTemporal_ListarPendientes'),
(N'dbo.sp_Reserva_Cancelar'),
(N'dbo.sp_Reserva_CheckIn'),
(N'dbo.sp_Reserva_CheckOut'),
(N'dbo.sp_Reserva_ConsultarSaldo'),
(N'dbo.sp_Reserva_Listar'),
(N'dbo.sp_Reserva_ListarPorEstado'),
(N'dbo.sp_Reserva_ListarPorFechas'),
(N'dbo.sp_Reserva_ListarPorHuesped'),
(N'dbo.sp_Rol_Listar'),
(N'dbo.sp_Servicio_Desactivar'),
(N'dbo.sp_Servicio_Insertar'),
(N'dbo.sp_Servicio_Listar'),
(N'dbo.sp_Servicio_ListarInactivos'),
(N'dbo.sp_Servicio_Modificar'),
(N'dbo.sp_Servicio_Reactivar'),
(N'dbo.sp_TipoHabitacion_Desactivar'),
(N'dbo.sp_TipoHabitacion_Insertar'),
(N'dbo.sp_TipoHabitacion_Listar'),
(N'dbo.sp_TipoHabitacion_Modificar'),
(N'dbo.sp_Usuario_BuscarPorDNI'),
(N'dbo.sp_Usuario_BuscarPorEmail'),
(N'dbo.sp_Usuario_CambiarContrasena'),
(N'dbo.sp_Usuario_Desactivar'),
(N'dbo.sp_Usuario_Insertar'),
(N'dbo.sp_Usuario_Listar'),
(N'dbo.sp_Usuario_Modificar'),
(N'dbo.sp_Usuario_Reactivar');
SELECT nombre,HAS_PERMS_BY_NAME(nombre,'OBJECT','EXECUTE') AS ejecutar FROM @Rutas;
IF EXISTS(SELECT 1 FROM @Rutas WHERE COALESCE(HAS_PERMS_BY_NAME(nombre,'OBJECT','EXECUTE'),0)<>1)
 THROW 51802, 'Faltan rutas EXECUTE requeridas.', 1;
DECLARE @Tablas TABLE(nombre sysname);
INSERT @Tablas VALUES
(N'dbo.Roles'),(N'dbo.Usuarios'),(N'dbo.Huespedes'),(N'dbo.Habitaciones'),(N'dbo.TiposHabitacion'),(N'dbo.Reservas'),(N'dbo.ReservasTemporales'),(N'dbo.Pagos'),(N'dbo.Servicios'),(N'dbo.DetalleReservas'),(N'dbo.DetalleServicios');
SELECT t.nombre,p.permiso,HAS_PERMS_BY_NAME(t.nombre,'OBJECT',p.permiso) AS permiso_directo
FROM @Tablas t CROSS JOIN (VALUES('SELECT'),('INSERT'),('UPDATE'),('DELETE'),('ALTER'),('CONTROL')) p(permiso);
IF EXISTS(SELECT 1 FROM @Tablas t CROSS JOIN (VALUES('SELECT'),('INSERT'),('UPDATE'),('DELETE'),('ALTER'),('CONTROL')) p(permiso)
 WHERE COALESCE(HAS_PERMS_BY_NAME(t.nombre,'OBJECT',p.permiso),1)<>0)
 THROW 51803, 'Existe acceso directo o visibilidad insuficiente.', 1;
-- Detecta el caso especial de GRANT a columnas que puede prevalecer sobre DENY de tabla.
IF EXISTS(SELECT 1 FROM @Tablas t JOIN sys.columns c ON c.object_id=OBJECT_ID(t.nombre)
 CROSS JOIN (VALUES('SELECT'),('UPDATE')) p(permiso)
 WHERE COALESCE(HAS_PERMS_BY_NAME(t.nombre,'OBJECT',p.permiso,c.name,'COLUMN'),1)<>0)
 THROW 51804, 'Permisos directos de columna incompatibles o indeterminados.', 1;
SELECT N'Permisos de objetos comprobados; falta ensayo funcional con token real y auditoria de servidor' AS resultado;
