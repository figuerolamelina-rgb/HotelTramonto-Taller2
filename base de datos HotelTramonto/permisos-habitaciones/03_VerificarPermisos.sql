-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- SOLO LECTURA. Ejecutar como la identidad Windows REAL de la aplicación, después de 02.
SELECT ORIGINAL_LOGIN() AS login_original, USER_NAME() AS usuario_base,
       IS_SRVROLEMEMBER('sysadmin') AS es_sysadmin, IS_MEMBER('db_owner') AS es_db_owner;
SELECT 'dbo.Habitaciones' AS objeto,
       HAS_PERMS_BY_NAME('dbo.Habitaciones', 'OBJECT', 'INSERT') AS insertar_directo,
       HAS_PERMS_BY_NAME('dbo.Habitaciones', 'OBJECT', 'UPDATE') AS actualizar_directo,
       HAS_PERMS_BY_NAME('dbo.Habitaciones', 'OBJECT', 'DELETE') AS eliminar_directo;
SELECT 'dbo.TiposHabitacion' AS objeto,
       HAS_PERMS_BY_NAME('dbo.TiposHabitacion', 'OBJECT', 'UPDATE') AS actualizar_directo;
-- Las escrituras directas anteriores deben ser 0. NULL requiere diagnóstico, no indica éxito.
-- EXECUTE debe ser 1 para TODAS las rutas de Datos (incluida administración compartida).
SELECT p.nombre, HAS_PERMS_BY_NAME(N'dbo.' + p.nombre, 'OBJECT', 'EXECUTE') AS puede_ejecutar
FROM (VALUES
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
    (N'sp_Usuario_Reactivar')) p(nombre);
-- La consulta de permisos no verifica que el cuerpo del procedimiento funcione.
-- Revisar permisos elevados, suplantación, grupos y GRANT de columna además de estos resultados.
SELECT * FROM sys.fn_my_permissions(NULL, 'DATABASE');

GO
