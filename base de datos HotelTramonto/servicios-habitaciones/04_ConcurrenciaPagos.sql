IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
 THROW 51400, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51401, 'Ejecutar sin transaccion externa.', 1;
-- Instalar DESPUES de 01_Instalar, con todos los clientes detenidos y respaldo.
IF COL_LENGTH('dbo.DetalleServicios','id_operacion') IS NULL
 THROW 51430, 'Falta instalar 01_Instalar.sql.', 1;
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN (
OBJECT_ID('dbo.sp_Pago_Insertar'),OBJECT_ID('dbo.sp_Pago_ActualizarEstado'),OBJECT_ID('dbo.sp_Pago_RegistrarServicios'),OBJECT_ID('dbo.sp_Pago_RegistrarSaldoAlojamiento'),OBJECT_ID('dbo.sp_DetalleServicio_Insertar'),OBJECT_ID('dbo.sp_Reserva_CheckOut'),OBJECT_ID('dbo.sp_ReservaTemporal_ConsultarConfirmacion')))
 THROW 51431, 'Hay modulos firmados; coordinar con DBA.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk WHERE fk.parent_object_id=OBJECT_ID('dbo.DetalleServicios')
 AND fk.name='FK_DetalleServicios_ReservaHabitacion' AND fk.referenced_object_id=OBJECT_ID('dbo.DetalleReservas')
 AND fk.is_disabled=0 AND fk.is_not_trusted=0 AND fk.update_referential_action=0 AND fk.delete_referential_action=0
 AND (SELECT COUNT(*) FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id)=2
 AND EXISTS (SELECT 1 FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id AND c.constraint_column_id=1
 AND COL_NAME(c.parent_object_id,c.parent_column_id)='id_reserva' AND COL_NAME(c.referenced_object_id,c.referenced_column_id)='id_reserva')
 AND EXISTS (SELECT 1 FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id AND c.constraint_column_id=2
 AND COL_NAME(c.parent_object_id,c.parent_column_id)='id_habitacion' AND COL_NAME(c.referenced_object_id,c.referenced_column_id)='id_habitacion'))
 THROW 51308, 'Clave foranea incompatible o no confiable.', 1;

BEGIN TRY
 BEGIN TRANSACTION;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Pagos') AND name='IX_Pagos_SaldoReserva')
 CREATE INDEX IX_Pagos_SaldoReserva ON dbo.Pagos(id_reserva,tipo_pago,estado) INCLUDE(monto);
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Pagos') AND name='IX_Pagos_SaldoTemporal')
 CREATE INDEX IX_Pagos_SaldoTemporal ON dbo.Pagos(id_reserva_temporal,tipo_pago,estado) INCLUDE(monto);
 IF COL_LENGTH('dbo.Pagos','id_operacion') IS NULL EXEC(N'ALTER TABLE dbo.Pagos ADD id_operacion UNIQUEIDENTIFIER NULL;');
 IF COL_LENGTH('dbo.Pagos','solicitud_hash') IS NULL EXEC(N'ALTER TABLE dbo.Pagos ADD solicitud_hash VARBINARY(32) NULL;');
 IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Pagos') AND name='id_operacion' AND system_type_id=36 AND is_nullable=1)
 OR NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Pagos') AND name='solicitud_hash' AND system_type_id=165 AND max_length=32 AND is_nullable=1)
 THROW 51432, 'Columnas de idempotencia incompatibles.', 1;
 IF OBJECT_ID('dbo.CK_Pagos_OperacionSolicitud','C') IS NULL
 EXEC(N'ALTER TABLE dbo.Pagos WITH CHECK ADD CONSTRAINT CK_Pagos_OperacionSolicitud CHECK
 ((id_operacion IS NULL AND solicitud_hash IS NULL) OR
 (id_operacion IS NOT NULL AND solicitud_hash IS NOT NULL));');
 IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('dbo.Pagos')
 AND name='CK_Pagos_OperacionSolicitud' AND is_disabled=0 AND is_not_trusted=0
 AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(definition,'[',''),']',''),'(',''),')',''),' ',''),CHAR(13),''),CHAR(10),''),CHAR(9),'')
 = 'id_operacionISNULLANDsolicitud_hashISNULLORid_operacionISNOTNULLANDsolicitud_hashISNOTNULL')
 THROW 51432, 'CHECK de identidad incompatible.', 1;
 IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Pagos') AND name='UX_Pagos_Operacion')
 EXEC(N'CREATE UNIQUE INDEX UX_Pagos_Operacion ON dbo.Pagos(id_operacion) WHERE id_operacion IS NOT NULL;');
 IF NOT EXISTS(SELECT 1 FROM sys.indexes i WHERE i.object_id=OBJECT_ID('dbo.Pagos') AND name='UX_Pagos_Operacion'
 AND is_unique=1 AND is_disabled=0 AND has_filter=1
 AND REPLACE(REPLACE(REPLACE(REPLACE(filter_definition,'[',''),']',''),'(',''),')','')='id_operacion IS NOT NULL'
 AND (SELECT COUNT(*) FROM sys.index_columns c WHERE c.object_id=i.object_id AND c.index_id=i.index_id AND c.key_ordinal>0)=1
 AND EXISTS(SELECT 1 FROM sys.index_columns c WHERE c.object_id=i.object_id AND c.index_id=i.index_id AND c.key_ordinal=1 AND COL_NAME(c.object_id,c.column_id)='id_operacion'))
 THROW 51433, 'Indice de operaciones incompatible.', 1;
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_Insertar
 @id_reserva INT=NULL, @id_reserva_temporal INT=NULL, @id_usuario INT,
 @monto DECIMAL(10,2), @metodo_pago VARCHAR(30), @estado VARCHAR(20),
 @tipo_pago VARCHAR(30), @id_operacion UNIQUEIDENTIFIER=NULL
AS
BEGIN
 SET NOCOUNT ON;
 SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51401, ''No se admite transaccion externa.'', 1;
 IF (@id_reserva IS NULL AND @id_reserva_temporal IS NULL) OR
    (@id_reserva IS NOT NULL AND @id_reserva_temporal IS NOT NULL) OR
    @id_reserva<=0 OR @id_reserva_temporal<=0 OR @id_usuario IS NULL OR @id_usuario<=0 OR
    @id_operacion IS NULL OR @id_operacion=''00000000-0000-0000-0000-000000000000'' OR
    @metodo_pago IS NULL OR LEN(LTRIM(RTRIM(@metodo_pago)))=0 OR
    @estado IS NULL OR @estado NOT IN (''PENDIENTE'',''APROBADO'',''RECHAZADO'') OR
    @tipo_pago IS NULL OR @tipo_pago NOT IN (''PAGO_INICIAL'',''SALDO_ALOJAMIENTO'',''SERVICIOS_ADICIONALES'') OR
    (@monto IS NOT NULL AND @monto<=0)
  THROW 51402, ''Solicitud de pago invalida o sin identificador de operacion.'', 1;
 -- NULL monto means "collect current balance" and is part of the immutable request.
 DECLARE @solicitud NVARCHAR(MAX)=(SELECT @id_reserva AS reserva,@id_reserva_temporal AS temporal,
  @id_usuario AS usuario,@monto AS monto,@metodo_pago AS metodo,@estado AS estado,@tipo_pago AS tipo
  FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER);
 DECLARE @hash VARBINARY(32)=HASHBYTES(''SHA2_256'',@solicitud), @existente INT, @hash_previo VARBINARY(32);

 DECLARE @estado_reserva VARCHAR(20), @total_alojamiento DECIMAL(10,2), @fecha_cierre DATETIME,
 @estado_temporal VARCHAR(20), @definitiva INT, @expiracion DATETIME,
 @habitacion INT, @entrada DATE, @salida DATE, @modalidad VARCHAR(20),
 @precio DECIMAL(10,2), @total DECIMAL(38,2), @pagado DECIMAL(38,2), @saldo DECIMAL(38,2),
 @recurso NVARCHAR(255), @bloqueo INT;

 BEGIN TRY
  BEGIN TRANSACTION;
  -- Operation lock precedes parent lock. Different requests never share an operation key.
  SET @recurso=CONCAT(N''HotelTramonto:pago-operacion:'',CONVERT(NVARCHAR(36),@id_operacion));
  EXEC @bloqueo=sys.sp_getapplock @Resource=@recurso,@LockMode=''Exclusive'',@LockOwner=''Transaction'',@LockTimeout=10000;
  IF @bloqueo<0 THROW 51421, ''No se pudo bloquear la operacion.'', 1;
  SELECT @existente=id_pago,@hash_previo=solicitud_hash FROM dbo.Pagos WHERE id_operacion=@id_operacion;
  IF @existente IS NOT NULL
  BEGIN
   IF @hash_previo<>@hash OR @hash_previo IS NULL THROW 51403, ''Operacion reutilizada con otros datos.'', 1;
   COMMIT;
   SELECT id_pago,id_reserva,monto AS monto_pagado,tipo_pago,metodo_pago,estado FROM dbo.Pagos WHERE id_pago=@existente;
   RETURN;
  END;

 SET @recurso=CASE WHEN @id_reserva IS NOT NULL THEN CONCAT(N''HotelTramonto:reserva:'',@id_reserva)
                  ELSE CONCAT(N''HotelTramonto:temporal:'',@id_reserva_temporal) END;
 EXEC @bloqueo=sys.sp_getapplock @Resource=@recurso,@LockMode=''Exclusive'',@LockOwner=''Transaction'',@LockTimeout=10000;
 IF @bloqueo<0 THROW 51421, ''No se pudo bloquear la reserva.'', 1;
 IF @id_reserva IS NOT NULL
 BEGIN
  SELECT @estado_reserva=estado,@total_alojamiento=total,@fecha_cierre=fecha_checkout FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK)
   WHERE id_reserva=@id_reserva;
  IF @estado_reserva IS NULL THROW 51422, ''La reserva no existe.'', 1;
 END
 ELSE
 BEGIN
  SELECT @estado_temporal=estado,@definitiva=id_reserva,@expiracion=fecha_expiracion,
   @habitacion=id_habitacion,@entrada=fecha_entrada,@salida=fecha_salida,@modalidad=modalidad_pago
   FROM dbo.ReservasTemporales WITH (UPDLOCK,HOLDLOCK) WHERE id_reserva_temporal=@id_reserva_temporal;
  IF @estado_temporal IS NULL THROW 51423, ''La temporal no existe.'', 1;
 END;

 IF NOT EXISTS(SELECT 1 FROM dbo.Usuarios WHERE id_usuario=@id_usuario AND activo=1)
  THROW 51404, ''Usuario responsable invalido.'', 1;

 IF @id_reserva IS NOT NULL
 BEGIN
  IF @tipo_pago=''SERVICIOS_ADICIONALES''
  BEGIN
   IF @estado_reserva<>''CHECKIN'' OR @fecha_cierre IS NOT NULL THROW 51410, ''Servicios requiere CHECKIN.'', 1;
   SELECT @total=COALESCE(SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario),0)
    FROM dbo.DetalleServicios WITH (HOLDLOCK) WHERE id_reserva=@id_reserva;
   SELECT @pagado=COALESCE(SUM(CAST(monto AS DECIMAL(19,2))),0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva=@id_reserva AND estado=''APROBADO'' AND tipo_pago=''SERVICIOS_ADICIONALES'';
  END
  ELSE IF @tipo_pago=''SALDO_ALOJAMIENTO''
  BEGIN
   IF @estado_reserva<>''CONFIRMADA'' OR @fecha_cierre IS NOT NULL THROW 51411, ''Saldo de alojamiento requiere CONFIRMADA.'', 1;
   SET @total=@total_alojamiento;
   SELECT @pagado=COALESCE(SUM(CAST(monto AS DECIMAL(19,2))),0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva=@id_reserva AND estado=''APROBADO'' AND tipo_pago IN (''PAGO_INICIAL'',''SALDO_ALOJAMIENTO'');
  END
  ELSE THROW 51412, ''Tipo de pago incompatible con reserva definitiva.'', 1;
  SET @saldo=@total-@pagado;
  IF @saldo<=0 THROW 51413, ''No existe saldo pendiente.'', 1;
  IF @monto IS NULL SET @monto=CAST(@saldo AS DECIMAL(10,2));
  IF @monto>@saldo THROW 51414, ''El monto supera el saldo actualizado.'', 1;
 END
 ELSE
 BEGIN
  IF @tipo_pago<>''PAGO_INICIAL'' THROW 51415, ''La temporal solo admite pago inicial.'', 1;
  IF @estado_temporal<>''PENDIENTE'' OR @definitiva IS NOT NULL OR @expiracion<=GETDATE()
   THROW 51416, ''La reserva temporal no esta pendiente o vigente.'', 1;
  SELECT @precio=th.precio_noche FROM dbo.Habitaciones h WITH (UPDLOCK,HOLDLOCK)
   JOIN dbo.TiposHabitacion th WITH (HOLDLOCK) ON th.id_tipoHabitacion=h.tipo_habitacion
   WHERE h.id_habitacion=@habitacion AND h.activo=1 AND th.activo=1 AND h.estado<>''MANTENIMIENTO'';
  IF @precio IS NULL THROW 51417, ''Habitacion no disponible.'', 1;
  SET @total=CAST(DATEDIFF(DAY,@entrada,@salida) AS DECIMAL(19,2))*@precio;
  IF @modalidad=''SENA_50'' SET @total=ROUND(@total*0.50,2);
  ELSE IF @modalidad<>''PAGO_TOTAL'' OR @modalidad IS NULL THROW 51418, ''Modalidad invalida.'', 1;
  IF @monto IS NULL OR @monto<>@total THROW 51419, ''Pago inicial debe corresponder al 50 o 100 por ciento.'', 1;
  IF EXISTS(SELECT 1 FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
   WHERE id_reserva_temporal=@id_reserva_temporal AND tipo_pago=''PAGO_INICIAL'' AND estado=''APROBADO'')
   THROW 51420, ''La temporal ya tiene un pago inicial aprobado.'', 1;
 END;

 INSERT dbo.Pagos(id_reserva,id_reserva_temporal,id_usuario,monto,metodo_pago,estado,tipo_pago,id_operacion,solicitud_hash)
 VALUES(@id_reserva,@id_reserva_temporal,@id_usuario,@monto,@metodo_pago,@estado,@tipo_pago,@id_operacion,@hash);
 DECLARE @nuevo INT=CAST(SCOPE_IDENTITY() AS INT);
 COMMIT;
 SELECT id_pago,id_reserva,monto AS monto_pagado,tipo_pago,metodo_pago,estado FROM dbo.Pagos WHERE id_pago=@nuevo;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;';
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_ActualizarEstado
 @id_pago INT, @estado VARCHAR(20)
AS
BEGIN
 SET NOCOUNT ON;
 SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51401, ''No se admite transaccion externa.'', 1;
 IF @id_pago IS NULL OR @id_pago<=0 OR @estado IS NULL OR @estado NOT IN (''APROBADO'',''RECHAZADO'')
  THROW 50121, ''Solo se permite aprobar o rechazar un pago pendiente.'', 1;
 DECLARE @id_reserva INT,@id_reserva_temporal INT,@monto DECIMAL(10,2),@tipo_pago VARCHAR(30),
  @actual VARCHAR(20),@reserva_leida INT,@temporal_leida INT;
 -- Routing lookup only. No payment lock before acquiring the parent.
 SELECT @id_reserva=id_reserva,@id_reserva_temporal=id_reserva_temporal FROM dbo.Pagos WHERE id_pago=@id_pago;
 IF @id_reserva IS NULL AND @id_reserva_temporal IS NULL THROW 50120, ''El pago no existe.'', 1;

 DECLARE @estado_reserva VARCHAR(20), @total_alojamiento DECIMAL(10,2), @fecha_cierre DATETIME,
 @estado_temporal VARCHAR(20), @definitiva INT, @expiracion DATETIME,
 @habitacion INT, @entrada DATE, @salida DATE, @modalidad VARCHAR(20),
 @precio DECIMAL(10,2), @total DECIMAL(38,2), @pagado DECIMAL(38,2), @saldo DECIMAL(38,2),
 @recurso NVARCHAR(255), @bloqueo INT;

 BEGIN TRY
  BEGIN TRANSACTION;

 SET @recurso=CASE WHEN @id_reserva IS NOT NULL THEN CONCAT(N''HotelTramonto:reserva:'',@id_reserva)
                  ELSE CONCAT(N''HotelTramonto:temporal:'',@id_reserva_temporal) END;
 EXEC @bloqueo=sys.sp_getapplock @Resource=@recurso,@LockMode=''Exclusive'',@LockOwner=''Transaction'',@LockTimeout=10000;
 IF @bloqueo<0 THROW 51421, ''No se pudo bloquear la reserva.'', 1;
 IF @id_reserva IS NOT NULL
 BEGIN
  SELECT @estado_reserva=estado,@total_alojamiento=total,@fecha_cierre=fecha_checkout FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK)
   WHERE id_reserva=@id_reserva;
  IF @estado_reserva IS NULL THROW 51422, ''La reserva no existe.'', 1;
 END
 ELSE
 BEGIN
  SELECT @estado_temporal=estado,@definitiva=id_reserva,@expiracion=fecha_expiracion,
   @habitacion=id_habitacion,@entrada=fecha_entrada,@salida=fecha_salida,@modalidad=modalidad_pago
   FROM dbo.ReservasTemporales WITH (UPDLOCK,HOLDLOCK) WHERE id_reserva_temporal=@id_reserva_temporal;
  IF @estado_temporal IS NULL THROW 51423, ''La temporal no existe.'', 1;
 END;

 IF @id_reserva_temporal IS NOT NULL
  SELECT @precio=th.precio_noche FROM dbo.Habitaciones h WITH (UPDLOCK,HOLDLOCK)
   JOIN dbo.TiposHabitacion th WITH (HOLDLOCK) ON th.id_tipoHabitacion=h.tipo_habitacion
   WHERE h.id_habitacion=@habitacion AND h.activo=1 AND th.activo=1 AND h.estado<>''MANTENIMIENTO'';
 SELECT @reserva_leida=id_reserva,@temporal_leida=id_reserva_temporal,@actual=estado,
  @monto=monto,@tipo_pago=tipo_pago FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK) WHERE id_pago=@id_pago;
 IF ISNULL(@reserva_leida,-1)<>ISNULL(@id_reserva,-1) OR ISNULL(@temporal_leida,-1)<>ISNULL(@id_reserva_temporal,-1)
  THROW 51424, ''El vinculo cambio durante la confirmacion. Reintente.'', 1;
 IF @actual=@estado BEGIN COMMIT; RETURN; END;
 IF @actual<>''PENDIENTE'' THROW 50122, ''Solo se puede actualizar un pago pendiente.'', 1;
 IF @estado=''APROBADO''
 BEGIN

 IF @id_reserva IS NOT NULL
 BEGIN
  IF @tipo_pago=''SERVICIOS_ADICIONALES''
  BEGIN
   IF @estado_reserva<>''CHECKIN'' OR @fecha_cierre IS NOT NULL THROW 51410, ''Servicios requiere CHECKIN.'', 1;
   SELECT @total=COALESCE(SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario),0)
    FROM dbo.DetalleServicios WITH (HOLDLOCK) WHERE id_reserva=@id_reserva;
   SELECT @pagado=COALESCE(SUM(CAST(monto AS DECIMAL(19,2))),0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva=@id_reserva AND estado=''APROBADO'' AND tipo_pago=''SERVICIOS_ADICIONALES'';
  END
  ELSE IF @tipo_pago=''SALDO_ALOJAMIENTO''
  BEGIN
   IF @estado_reserva<>''CONFIRMADA'' OR @fecha_cierre IS NOT NULL THROW 51411, ''Saldo de alojamiento requiere CONFIRMADA.'', 1;
   SET @total=@total_alojamiento;
   SELECT @pagado=COALESCE(SUM(CAST(monto AS DECIMAL(19,2))),0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva=@id_reserva AND estado=''APROBADO'' AND tipo_pago IN (''PAGO_INICIAL'',''SALDO_ALOJAMIENTO'');
  END
  ELSE THROW 51412, ''Tipo de pago incompatible con reserva definitiva.'', 1;
  SET @saldo=@total-@pagado;
  IF @saldo<=0 THROW 51413, ''No existe saldo pendiente.'', 1;
  IF @monto IS NULL SET @monto=CAST(@saldo AS DECIMAL(10,2));
  IF @monto>@saldo THROW 51414, ''El monto supera el saldo actualizado.'', 1;
 END
 ELSE
 BEGIN
  IF @tipo_pago<>''PAGO_INICIAL'' THROW 51415, ''La temporal solo admite pago inicial.'', 1;
  IF @estado_temporal<>''PENDIENTE'' OR @definitiva IS NOT NULL OR @expiracion<=GETDATE()
   THROW 51416, ''La reserva temporal no esta pendiente o vigente.'', 1;
  SELECT @precio=th.precio_noche FROM dbo.Habitaciones h WITH (UPDLOCK,HOLDLOCK)
   JOIN dbo.TiposHabitacion th WITH (HOLDLOCK) ON th.id_tipoHabitacion=h.tipo_habitacion
   WHERE h.id_habitacion=@habitacion AND h.activo=1 AND th.activo=1 AND h.estado<>''MANTENIMIENTO'';
  IF @precio IS NULL THROW 51417, ''Habitacion no disponible.'', 1;
  SET @total=CAST(DATEDIFF(DAY,@entrada,@salida) AS DECIMAL(19,2))*@precio;
  IF @modalidad=''SENA_50'' SET @total=ROUND(@total*0.50,2);
  ELSE IF @modalidad<>''PAGO_TOTAL'' OR @modalidad IS NULL THROW 51418, ''Modalidad invalida.'', 1;
  IF @monto IS NULL OR @monto<>@total THROW 51419, ''Pago inicial debe corresponder al 50 o 100 por ciento.'', 1;
  IF EXISTS(SELECT 1 FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
   WHERE id_reserva_temporal=@id_reserva_temporal AND tipo_pago=''PAGO_INICIAL'' AND estado=''APROBADO'')
   THROW 51420, ''La temporal ya tiene un pago inicial aprobado.'', 1;
 END;

 END;
 UPDATE dbo.Pagos SET estado=@estado WHERE id_pago=@id_pago;
 COMMIT;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;';
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarServicios
 @id_reserva INT,@id_usuario INT,@metodo_pago VARCHAR(30),@id_operacion UNIQUEIDENTIFIER=NULL
AS
BEGIN
 SET NOCOUNT ON;
 EXEC dbo.sp_Pago_Insertar @id_reserva=@id_reserva,@id_reserva_temporal=NULL,
  @id_usuario=@id_usuario,@monto=NULL,@metodo_pago=@metodo_pago,@estado=''APROBADO'',
  @tipo_pago=''SERVICIOS_ADICIONALES'',@id_operacion=@id_operacion;
END;';
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarSaldoAlojamiento
 @id_reserva INT,@id_usuario INT,@metodo_pago VARCHAR(30),@id_operacion UNIQUEIDENTIFIER=NULL
AS
BEGIN
 SET NOCOUNT ON;
 EXEC dbo.sp_Pago_Insertar @id_reserva=@id_reserva,@id_reserva_temporal=NULL,
  @id_usuario=@id_usuario,@monto=NULL,@metodo_pago=@metodo_pago,@estado=''APROBADO'',
  @tipo_pago=''SALDO_ALOJAMIENTO'',@id_operacion=@id_operacion;
END;';

 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_DetalleServicio_Insertar
 @id_servicios INT, @id_reserva INT, @cantidad INT,
 @precio_unitario DECIMAL(10,2), @id_habitacion INT, @id_operacion UNIQUEIDENTIFIER
AS
BEGIN
 SET NOCOUNT ON;
 SET XACT_ABORT ON;
 IF @@TRANCOUNT <> 0 THROW 51312, ''No se admite transaccion externa.'', 1;
 IF @id_reserva IS NULL OR @id_reserva<=0 OR @id_servicios IS NULL OR @id_servicios<=0
 OR @id_habitacion IS NULL OR @id_habitacion<=0 OR @cantidad IS NULL OR @cantidad<=0
 OR @precio_unitario IS NULL OR @precio_unitario<=0 OR @id_operacion IS NULL
 OR @id_operacion=''00000000-0000-0000-0000-000000000000''
 THROW 51313, ''Datos de consumo invalidos.'', 1;
 BEGIN TRY
  BEGIN TRANSACTION;
        DECLARE @bloqueo INT, @recurso NVARCHAR(255) = CONCAT(N''HotelTramonto:reserva:'', @id_reserva);
        EXEC @bloqueo = sys.sp_getapplock @Resource=@recurso,
             @LockMode=''Exclusive'', @LockOwner=''Transaction'', @LockTimeout=10000;
        IF @bloqueo < 0 THROW 51310, ''No se pudo bloquear la reserva. Reintente.'', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK) WHERE id_reserva=@id_reserva)
            THROW 51311, ''La reserva no existe.'', 1;

  -- Primero el reintento: funciona incluso si la reserva se cerro o cambio el catalogo.
  DECLARE @existente INT;
  SELECT @existente=id_detalle_servicio FROM dbo.DetalleServicios WITH (UPDLOCK,HOLDLOCK)
   WHERE id_operacion=@id_operacion;
  IF @existente IS NOT NULL
  BEGIN
   IF NOT EXISTS (SELECT 1 FROM dbo.DetalleServicios WHERE id_detalle_servicio=@existente
    AND id_reserva=@id_reserva AND id_habitacion=@id_habitacion AND id_servicios=@id_servicios
    AND cantidad=@cantidad AND precio_unitario=@precio_unitario)
    THROW 51314, ''Identificador de operacion reutilizado con otros datos.'', 1;
   COMMIT;
   RETURN;
  END;
  IF NOT EXISTS (SELECT 1 FROM dbo.Reservas WHERE id_reserva=@id_reserva AND estado=''CHECKIN'' AND fecha_checkout IS NULL)
   THROW 51315, ''La reserva debe estar en CHECKIN.'', 1;
  IF NOT EXISTS (SELECT 1 FROM dbo.DetalleReservas WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva=@id_reserva AND id_habitacion=@id_habitacion)
   THROW 51316, ''La habitacion no pertenece a la reserva.'', 1;
  DECLARE @precio DECIMAL(10,2);
  SELECT @precio=precio FROM dbo.Servicios WITH (HOLDLOCK)
    WHERE id_servicio=@id_servicios AND activo=1 AND categoria<>''Incluido'';
  IF @precio IS NULL OR @precio<=0 OR @precio<>@precio_unitario
   THROW 51317, ''Servicio no disponible o precio modificado. Actualice el catalogo.'', 1;
  IF CAST(@cantidad AS DECIMAL(19,2))*@precio>99999999.99
   THROW 51318, ''El subtotal supera el importe permitido.'', 1;
  IF (SELECT COALESCE(SUM(CAST(cantidad AS DECIMAL(19,2))*precio_unitario),0)
      FROM dbo.DetalleServicios WITH (HOLDLOCK) WHERE id_reserva=@id_reserva)
      + CAST(@cantidad AS DECIMAL(19,2))*@precio > 99999999.99
   THROW 51318, ''El total de servicios supera el importe permitido.'', 1;
  INSERT dbo.DetalleServicios(id_servicios,id_reserva,id_habitacion,cantidad,precio_unitario,id_operacion)
  VALUES(@id_servicios,@id_reserva,@id_habitacion,@cantidad,@precio,@id_operacion);
  COMMIT;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;';
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Reserva_CheckOut
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;

    SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51312, ''No se admite transaccion externa.'', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @bloqueo INT, @recurso NVARCHAR(255) = CONCAT(N''HotelTramonto:reserva:'', @id_reserva);
        EXEC @bloqueo = sys.sp_getapplock @Resource=@recurso,
             @LockMode=''Exclusive'', @LockOwner=''Transaction'', @LockTimeout=10000;
        IF @bloqueo < 0 THROW 51310, ''No se pudo bloquear la reserva. Reintente.'', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Reservas WITH (UPDLOCK,HOLDLOCK) WHERE id_reserva=@id_reserva)
            THROW 51311, ''La reserva no existe.'', 1;
    DECLARE
        @estado_reserva VARCHAR(20),
        @total_alojamiento DECIMAL(10,2),
        @pagado_alojamiento DECIMAL(10,2),
        @saldo_alojamiento DECIMAL(10,2),
        @total_servicios DECIMAL(10,2),
        @pagado_servicios DECIMAL(10,2),
        @saldo_servicios DECIMAL(10,2);

    SELECT
        @estado_reserva = estado,
        @total_alojamiento = total
    FROM Reservas
    WHERE id_reserva = @id_reserva;

    IF @estado_reserva IS NULL
        THROW 50029, ''La reserva no existe.'', 1;

    IF @estado_reserva = ''FINALIZADA''
    BEGIN
        COMMIT TRANSACTION;
        SELECT id_reserva,estado,fecha_checkout FROM dbo.Reservas WHERE id_reserva=@id_reserva;
        RETURN;
    END;
    IF @estado_reserva <> ''CHECKIN''
        THROW 50030, ''La reserva debe estar en CHECKIN para realizar el check-out.'', 1;

    -- saldo de alojamiento
    SELECT
        @pagado_alojamiento = ISNULL(SUM(monto), 0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago IN (''PAGO_INICIAL'', ''SALDO_ALOJAMIENTO'');

    SET @saldo_alojamiento =
        @total_alojamiento - @pagado_alojamiento;

    -- saldo de servicios
    SELECT
        @total_servicios = ISNULL(SUM(cantidad * precio_unitario), 0)
    FROM dbo.DetalleServicios WITH (HOLDLOCK)
    WHERE id_reserva = @id_reserva;

    SELECT
        @pagado_servicios = ISNULL(SUM(monto), 0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago = ''SERVICIOS_ADICIONALES'';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_alojamiento > 0
        THROW 50031, ''No se puede realizar el check-out porque existe saldo de alojamiento pendiente.'', 1;

    IF @saldo_servicios > 0
        THROW 50032, ''No se puede realizar el check-out porque existen servicios pendientes de pago.'', 1;

    UPDATE Reservas
    SET
        estado = ''FINALIZADA'',
        fecha_checkout = GETDATE()
    WHERE id_reserva = @id_reserva;

    COMMIT TRANSACTION;
    SELECT
        id_reserva,
        estado,
        fecha_checkout
    FROM Reservas
    WHERE id_reserva = @id_reserva;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';
 EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_ReservaTemporal_ConsultarConfirmacion
 @id_reserva_temporal INT
AS
BEGIN
 SET NOCOUNT ON;
 IF @id_reserva_temporal IS NULL OR @id_reserva_temporal<=0 THROW 51440, ''Temporal invalida.'', 1;
 SELECT r.id_reserva,r.total AS total_alojamiento,
  COALESCE((SELECT SUM(p.monto) FROM dbo.Pagos p WHERE p.id_reserva=r.id_reserva
   AND p.tipo_pago=''PAGO_INICIAL'' AND p.estado=''APROBADO''),0) AS pago_inicial,
  r.total-COALESCE((SELECT SUM(p.monto) FROM dbo.Pagos p WHERE p.id_reserva=r.id_reserva
   AND p.tipo_pago=''PAGO_INICIAL'' AND p.estado=''APROBADO''),0) AS saldo_alojamiento
 FROM dbo.ReservasTemporales t JOIN dbo.Reservas r ON r.id_reserva=t.id_reserva
 WHERE t.id_reserva_temporal=@id_reserva_temporal AND t.estado=''CONFIRMADA'';
END;';
 COMMIT;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK;
 THROW;
END CATCH;
