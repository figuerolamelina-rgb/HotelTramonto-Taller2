-- SOLO METADATOS Y LECTURAS. No invoca procedimientos de escritura, migra ni concede permisos.
-- Seleccionar explícitamente HotelTramontoDB_Pruebas. No contiene USE.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51300, 'Solo se admite HotelTramontoDB_Pruebas.', 1;
END;
IF ISNULL(HAS_PERMS_BY_NAME(DB_NAME(),'DATABASE','VIEW DEFINITION'),0) <> 1
    THROW 51301, 'Se requiere visibilidad de definiciones para comprobar contratos.', 1;
DECLARE @Contratos table(procedimiento sysname,parametro sysname,tipo sysname,longitud int);
-- INSERT solo en variable local: no modifica tablas de la base.
INSERT INTO @Contratos VALUES
(N'sp_Habitacion_ConsultarDisponibilidad',N'@fecha_entrada',N'date',3),
(N'sp_Habitacion_ConsultarDisponibilidad',N'@fecha_salida',N'date',3),
(N'sp_Habitacion_Desactivar',N'@id_habitacion',N'int',4),
(N'sp_Habitacion_Insertar',N'@tipo_habitacion',N'int',4),
(N'sp_Habitacion_Insertar',N'@piso',N'int',4),
(N'sp_Habitacion_Insertar',N'@numero',N'varchar',10),
(N'sp_Habitacion_Insertar',N'@estado',N'varchar',20),
(N'sp_Habitacion_Listar',N'',N'',0),
(N'sp_Habitacion_ListarInactivas',N'',N'',0),
(N'sp_Habitacion_Modificar',N'@id_habitacion',N'int',4),
(N'sp_Habitacion_Modificar',N'@tipo_habitacion',N'int',4),
(N'sp_Habitacion_Modificar',N'@piso',N'int',4),
(N'sp_Habitacion_Modificar',N'@numero',N'varchar',10),
(N'sp_Habitacion_Modificar',N'@estado',N'varchar',20),
(N'sp_Habitacion_Reactivar',N'@id_habitacion',N'int',4),
(N'sp_Servicio_Desactivar',N'@id_servicio',N'int',4),
(N'sp_Servicio_Insertar',N'@nombre',N'varchar',50),
(N'sp_Servicio_Insertar',N'@descripcion',N'varchar',150),
(N'sp_Servicio_Insertar',N'@precio',N'decimal',9),
(N'sp_Servicio_Insertar',N'@categoria',N'varchar',30),
(N'sp_Servicio_Listar',N'',N'',0),
(N'sp_Servicio_ListarInactivos',N'',N'',0),
(N'sp_Servicio_Modificar',N'@id_servicio',N'int',4),
(N'sp_Servicio_Modificar',N'@nombre',N'varchar',50),
(N'sp_Servicio_Modificar',N'@descripcion',N'varchar',150),
(N'sp_Servicio_Modificar',N'@precio',N'decimal',9),
(N'sp_Servicio_Modificar',N'@categoria',N'varchar',30),
(N'sp_Servicio_Reactivar',N'@id_servicio',N'int',4),
(N'sp_TipoHabitacion_Desactivar',N'@id_tipoHabitacion',N'int',4),
(N'sp_TipoHabitacion_Insertar',N'@nombre',N'varchar',50),
(N'sp_TipoHabitacion_Insertar',N'@descripcion',N'varchar',200),
(N'sp_TipoHabitacion_Insertar',N'@precio_noche',N'decimal',9),
(N'sp_TipoHabitacion_Insertar',N'@capacidad',N'int',4),
(N'sp_TipoHabitacion_Listar',N'',N'',0),
(N'sp_TipoHabitacion_Modificar',N'@id_tipoHabitacion',N'int',4),
(N'sp_TipoHabitacion_Modificar',N'@nombre',N'varchar',50),
(N'sp_TipoHabitacion_Modificar',N'@descripcion',N'varchar',200),
(N'sp_TipoHabitacion_Modificar',N'@precio_noche',N'decimal',9),
(N'sp_TipoHabitacion_Modificar',N'@capacidad',N'int',4),
(N'sp_Usuario_BuscarPorEmail',N'@email',N'varchar',100),
(N'sp_Usuario_Reactivar',N'@id_usuario',N'int',4);
SELECT DISTINCT c.procedimiento,
 CASE WHEN OBJECT_ID(N'dbo.'+c.procedimiento,N'P') IS NULL THEN 'FALLO' ELSE 'APROBADA' END AS resultado,
 'Existencia y visibilidad' AS comprobacion
FROM @Contratos c;
SELECT c.procedimiento,c.parametro,c.tipo AS tipo_esperado,c.longitud AS longitud_esperada,
 CASE WHEN c.parametro=N'' THEN
   CASE WHEN EXISTS(SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID(N'dbo.'+c.procedimiento) AND parameter_id>0) THEN 'FALLO' ELSE 'APROBADA' END
 ELSE CASE WHEN p.name IS NOT NULL AND TYPE_NAME(p.user_type_id)=c.tipo AND p.max_length=c.longitud AND p.is_output=0
 THEN 'APROBADA' ELSE 'FALLO' END END AS resultado,
 'Parametro de entrada' AS comprobacion
FROM @Contratos c LEFT JOIN sys.parameters p ON p.object_id=OBJECT_ID(N'dbo.'+c.procedimiento) AND p.name=c.parametro;
SELECT n.procedimiento,p.name AS parametro_no_previsto,'FALLO' AS resultado
FROM (SELECT DISTINCT procedimiento FROM @Contratos) n
JOIN sys.parameters p ON p.object_id=OBJECT_ID(N'dbo.'+n.procedimiento) AND p.parameter_id>0
WHERE NOT EXISTS(SELECT 1 FROM @Contratos c WHERE c.procedimiento=n.procedimiento AND c.parametro=p.name);
-- Definiciones para comparación estricta local. No se devuelven filas ni hashes de usuarios.
SELECT DISTINCT c.procedimiento,OBJECT_DEFINITION(OBJECT_ID(N'dbo.'+c.procedimiento,N'P')) AS definicion
FROM @Contratos c;
DECLARE @Columnas table(procedimiento sysname,columna sysname);
INSERT INTO @Columnas VALUES
(N'sp_Habitacion_Listar',N'id_habitacion'),
(N'sp_Habitacion_Listar',N'tipo_habitacion'),
(N'sp_Habitacion_Listar',N'tipo'),
(N'sp_Habitacion_Listar',N'piso'),
(N'sp_Habitacion_Listar',N'numero'),
(N'sp_Habitacion_Listar',N'estado'),
(N'sp_Habitacion_Listar',N'activo'),
(N'sp_Habitacion_ListarInactivas',N'id_habitacion'),
(N'sp_Habitacion_ListarInactivas',N'tipo_habitacion'),
(N'sp_Habitacion_ListarInactivas',N'tipo'),
(N'sp_Habitacion_ListarInactivas',N'piso'),
(N'sp_Habitacion_ListarInactivas',N'numero'),
(N'sp_Habitacion_ListarInactivas',N'estado'),
(N'sp_Habitacion_ListarInactivas',N'activo'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'id_habitacion'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'numero'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'piso'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'estado'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'id_tipoHabitacion'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'tipo_habitacion'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'precio_noche'),
(N'sp_Habitacion_ConsultarDisponibilidad',N'capacidad'),
(N'sp_TipoHabitacion_Listar',N'id_tipoHabitacion'),
(N'sp_TipoHabitacion_Listar',N'nombre'),
(N'sp_TipoHabitacion_Listar',N'descripcion'),
(N'sp_TipoHabitacion_Listar',N'precio_noche'),
(N'sp_TipoHabitacion_Listar',N'capacidad'),
(N'sp_TipoHabitacion_Listar',N'activo'),
(N'sp_Servicio_Listar',N'id_servicio'),
(N'sp_Servicio_Listar',N'nombre'),
(N'sp_Servicio_Listar',N'descripcion'),
(N'sp_Servicio_Listar',N'categoria'),
(N'sp_Servicio_Listar',N'precio'),
(N'sp_Servicio_Listar',N'activo'),
(N'sp_Servicio_ListarInactivos',N'id_servicio'),
(N'sp_Servicio_ListarInactivos',N'nombre'),
(N'sp_Servicio_ListarInactivos',N'descripcion'),
(N'sp_Servicio_ListarInactivos',N'categoria'),
(N'sp_Servicio_ListarInactivos',N'precio'),
(N'sp_Servicio_ListarInactivos',N'activo');
SELECT c.procedimiento,c.columna,
 CASE WHEN EXISTS(SELECT 1 FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID(N'dbo.'+c.procedimiento),0) d
                  WHERE d.name=c.columna AND d.error_number IS NULL) THEN 'APROBADA' ELSE 'FALLO' END AS resultado,
 'Columna esperada de lectura (sin ejecutar el SP)' AS comprobacion
FROM @Columnas c;
SELECT ORIGINAL_LOGIN() AS login_sql,USER_NAME() AS usuario_base,
       IS_SRVROLEMEMBER('sysadmin') AS es_sysadmin,IS_MEMBER('db_owner') AS es_db_owner,
       HAS_PERMS_BY_NAME('dbo.Habitaciones','OBJECT','UPDATE') AS puede_update_directo,
       HAS_PERMS_BY_NAME('dbo.sp_Habitacion_Modificar','OBJECT','EXECUTE') AS puede_ejecutar_administracion,
       'INFORMATIVO: una identidad Windows compartida no distingue empleados del login C#.' AS limitacion;
