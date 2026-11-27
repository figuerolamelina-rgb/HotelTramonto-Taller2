IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
    THROW 51300, 'Solo se permite HotelTramontoDB_Pruebas.', 1;
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF @@TRANCOUNT <> 0 THROW 51301, 'Ejecutar sin transaccion externa.', 1;
IF COL_LENGTH('dbo.Pagos','id_operacion') IS NOT NULL
 THROW 51434, 'Concurrencia instalada: reinstalar con 04_ConcurrenciaPagos.sql, no restaurar etapas anteriores.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.Servicios')
 AND name='categoria' AND system_type_id=167 AND max_length=30 AND is_nullable=1)
 THROW 51302, 'Instalar primero la compatibilidad Servicios.categoria de correccion 2.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.sp_Servicio_Insertar') AND name='@categoria')
 OR NOT EXISTS (SELECT 1 FROM sys.parameters WHERE object_id=OBJECT_ID('dbo.sp_Servicio_Modificar') AND name='@categoria')
 THROW 51303, 'Faltan procedimientos compatibles con categoria.', 1;
IF EXISTS (SELECT 1 FROM dbo.Servicios WHERE categoria IS NULL OR categoria NOT IN
 ('Almuerzo','Merienda','Cena','Tragos','Pesca guiada','Incluido','Otros'))
 THROW 51304, 'Resolver clasificacion de servicios antes de continuar.', 1;
IF EXISTS(SELECT 1 FROM sys.crypt_properties WHERE class=1 AND major_id IN (OBJECT_ID('dbo.sp_DetalleServicio_Insertar'),OBJECT_ID('dbo.sp_DetalleServicio_ListarPorReserva'),OBJECT_ID('dbo.sp_Pago_RegistrarServicios'),OBJECT_ID('dbo.sp_Reserva_CheckOut'))) THROW 51305, 'Modulos firmados: coordinar con DBA.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('dbo.sp_Servicio_Listar'),0)
 WHERE name='categoria' AND error_number IS NULL)
 THROW 51309, 'sp_Servicio_Listar no devuelve categoria.', 1;
BEGIN TRY
 BEGIN TRANSACTION;

IF COL_LENGTH('dbo.DetalleServicios','id_habitacion') IS NULL
 EXEC(N'ALTER TABLE dbo.DetalleServicios ADD id_habitacion INT NULL;');
IF COL_LENGTH('dbo.DetalleServicios','id_operacion') IS NULL
 EXEC(N'ALTER TABLE dbo.DetalleServicios ADD id_operacion UNIQUEIDENTIFIER NULL;');
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID('dbo.DetalleServicios')
 AND name='FK_DetalleServicios_ReservaHabitacion')
 EXEC(N'ALTER TABLE dbo.DetalleServicios WITH CHECK ADD CONSTRAINT FK_DetalleServicios_ReservaHabitacion
 FOREIGN KEY(id_reserva,id_habitacion) REFERENCES dbo.DetalleReservas(id_reserva,id_habitacion);');
IF OBJECT_ID('dbo.CK_DetalleServicios_AsociacionOperacion','C') IS NULL
 EXEC(N'ALTER TABLE dbo.DetalleServicios WITH CHECK ADD CONSTRAINT CK_DetalleServicios_AsociacionOperacion
 CHECK ((id_habitacion IS NULL AND id_operacion IS NULL) OR
        (id_habitacion IS NOT NULL AND id_operacion IS NOT NULL));');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.DetalleServicios')
 AND name='UX_DetalleServicios_Operacion')
 EXEC(N'CREATE UNIQUE INDEX UX_DetalleServicios_Operacion ON dbo.DetalleServicios(id_operacion) WHERE id_operacion IS NOT NULL;');

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.DetalleServicios') AND name='id_habitacion' AND system_type_id=56 AND is_nullable=1)
 OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.DetalleServicios') AND name='id_operacion' AND system_type_id=36 AND is_nullable=1)
 THROW 51306, 'Columnas existentes incompatibles; no se alteran automaticamente.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('dbo.DetalleServicios')
 AND name='CK_DetalleServicios_AsociacionOperacion' AND is_disabled=0 AND is_not_trusted=0
 AND REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(definition,'[',''),']',''),'(',''),')',''),' ',''),CHAR(13),''),CHAR(10),''),CHAR(9),'')
 = 'id_habitacionISNULLANDid_operacionISNULLORid_habitacionISNOTNULLANDid_operacionISNOTNULL')
 THROW 51306, 'CHECK de asociacion incompatible o no confiable.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes i WHERE i.object_id=OBJECT_ID('dbo.DetalleServicios')
 AND i.name='UX_DetalleServicios_Operacion' AND i.is_unique=1 AND i.is_disabled=0 AND i.has_filter=1
 AND REPLACE(REPLACE(REPLACE(REPLACE(i.filter_definition,'[',''),']',''),'(',''),')','')='id_operacion IS NOT NULL'
 AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal>0)=1
 AND EXISTS (SELECT 1 FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
 WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal=1 AND c.name='id_operacion'))
 THROW 51307, 'Indice de operacion incompatible.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk WHERE fk.parent_object_id=OBJECT_ID('dbo.DetalleServicios')
 AND fk.name='FK_DetalleServicios_ReservaHabitacion' AND fk.referenced_object_id=OBJECT_ID('dbo.DetalleReservas')
 AND fk.is_disabled=0 AND fk.is_not_trusted=0 AND fk.update_referential_action=0 AND fk.delete_referential_action=0
 AND (SELECT COUNT(*) FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id)=2
 AND EXISTS (SELECT 1 FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id AND c.constraint_column_id=1
 AND COL_NAME(c.parent_object_id,c.parent_column_id)='id_reserva' AND COL_NAME(c.referenced_object_id,c.referenced_column_id)='id_reserva')
 AND EXISTS (SELECT 1 FROM sys.foreign_key_columns c WHERE c.constraint_object_id=fk.object_id AND c.constraint_column_id=2
 AND COL_NAME(c.parent_object_id,c.parent_column_id)='id_habitacion' AND COL_NAME(c.referenced_object_id,c.referenced_column_id)='id_habitacion'))
 THROW 51308, 'Clave foranea incompatible o no confiable.', 1;

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

EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_DetalleServicio_ListarPorReserva
 @id_reserva INT, @id_habitacion INT = NULL
AS
BEGIN
 SET NOCOUNT ON;
 IF @id_reserva IS NULL OR @id_reserva<=0 OR @id_habitacion<=0
  THROW 51319, ''Identificadores invalidos.'', 1;
 SELECT DS.id_detalle_servicio, DS.id_reserva, DS.id_servicios, DS.id_habitacion,
 COALESCE(H.numero, ''Sin identificar'') AS numero_habitacion,
 S.nombre AS nombre_servicio, DS.cantidad, DS.precio_unitario,
 DS.cantidad*DS.precio_unitario AS subtotal
 FROM dbo.DetalleServicios DS
 INNER JOIN dbo.Servicios S ON S.id_servicio=DS.id_servicios
 LEFT JOIN dbo.Habitaciones H ON H.id_habitacion=DS.id_habitacion
 WHERE DS.id_reserva=@id_reserva AND (@id_habitacion IS NULL OR DS.id_habitacion=@id_habitacion)
 ORDER BY DS.id_detalle_servicio;
END;';

EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.sp_Pago_RegistrarServicios
    @id_reserva INT,
    @id_usuario INT,
    @metodo_pago VARCHAR(30)
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
        @total_servicios DECIMAL(10,2),
        @pagado_servicios DECIMAL(10,2),
        @saldo_servicios DECIMAL(10,2);

    -- calcular total consumido en servicios
    SELECT
        @total_servicios =
            ISNULL(SUM(cantidad * precio_unitario), 0)
    FROM dbo.DetalleServicios WITH (HOLDLOCK)
    WHERE id_reserva = @id_reserva;

    IF @total_servicios <= 0
        THROW 50024, ''La reserva no tiene servicios adicionales para pagar.'', 1;

    -- calcular lo ya pagado de servicios
    SELECT
        @pagado_servicios =
            ISNULL(SUM(monto), 0)
    FROM dbo.Pagos WITH (UPDLOCK,HOLDLOCK)
    WHERE id_reserva = @id_reserva
      AND estado = ''APROBADO''
      AND tipo_pago = ''SERVICIOS_ADICIONALES'';

    SET @saldo_servicios =
        @total_servicios - @pagado_servicios;

    IF @saldo_servicios <= 0
        THROW 50025, ''Los servicios adicionales ya están pagados.'', 1;

    -- registrar exactamente el saldo pendiente
    INSERT INTO Pagos
    (
        id_reserva,
        id_reserva_temporal,
        id_usuario,
        monto,
        metodo_pago,
        estado,
        tipo_pago
    )
    VALUES
    (
        @id_reserva,
        NULL,
        @id_usuario,
        @saldo_servicios,
        @metodo_pago,
        ''APROBADO'',
        ''SERVICIOS_ADICIONALES''
    );

    COMMIT TRANSACTION;
    SELECT
        SCOPE_IDENTITY() AS id_pago,
        @id_reserva AS id_reserva,
        @saldo_servicios AS monto_pagado,
        ''SERVICIOS_ADICIONALES'' AS tipo_pago;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
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

 COMMIT TRANSACTION;
END TRY
BEGIN CATCH
 IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
 THROW;
END CATCH;
