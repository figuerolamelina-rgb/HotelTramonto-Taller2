-- Seleccionar HotelTramontoDB_Pruebas en SSMS. Este script NO cambia de base.
IF DB_NAME() COLLATE Latin1_General_100_BIN2 <> N'HotelTramontoDB_Pruebas'
BEGIN
    THROW 51200, 'Base incorrecta. Solo se permite HotelTramontoDB_Pruebas.', 1;
END;
-- CLASIFICACIÓN EXPLÍCITA confirmada por el hotel: servicio 1 -> Otros.
-- Regla: los servicios adicionales se cobran separados del alojamiento.
-- No es un valor predeterminado para futuros servicios. No cambia precio ni activo.
-- Revisar el ID en la copia antes de ejecutar; no se reactiva el servicio.
SET XACT_ABORT ON;
IF @@TRANCOUNT<>0 THROW 51110, 'Ejecutar sin transacción externa.', 1;
IF COL_LENGTH(N'dbo.Servicios',N'categoria') IS NULL THROW 51111, 'Ejecutar primero 01.', 1;
DECLARE @Clasificaciones table(id_servicio INT PRIMARY KEY, categoria VARCHAR(100) NOT NULL);
INSERT INTO @Clasificaciones(id_servicio,categoria) VALUES (1, 'Otros');
IF (SELECT COUNT(*) FROM @Clasificaciones) <> 1
   OR NOT EXISTS(SELECT 1 FROM @Clasificaciones WHERE id_servicio=1 AND categoria COLLATE Latin1_General_100_BIN2='Otros' AND DATALENGTH(categoria)=5)
    THROW 51112, 'Este script solo permite clasificar ID 1 como Otros.', 1;
IF EXISTS(SELECT 1 FROM @Clasificaciones WHERE categoria COLLATE Latin1_General_100_BIN2 NOT IN
 ('Almuerzo','Merienda','Cena','Tragos','Pesca guiada','Incluido','Otros') OR DATALENGTH(categoria)>30 OR DATALENGTH(categoria)<>DATALENGTH(RTRIM(categoria)))
    THROW 51113, 'Categoría inválida. Usar exactamente uno de los siete valores del programa.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS(SELECT 1 FROM dbo.Servicios WITH(UPDLOCK,HOLDLOCK)
                  WHERE id_servicio=1 AND nombre='Desayuno Buffet' AND precio=9500.00 AND activo=0)
        THROW 51116, 'El servicio 1 no coincide con el escenario confirmado. No se modifica nada.', 1;
    IF EXISTS(SELECT 1 FROM @Clasificaciones c LEFT JOIN dbo.Servicios s WITH(UPDLOCK,HOLDLOCK)
              ON s.id_servicio=c.id_servicio WHERE s.id_servicio IS NULL)
        THROW 51114, 'Algún servicio no existe. No se aplica el lote.', 1;
    IF EXISTS(SELECT 1 FROM @Clasificaciones c JOIN dbo.Servicios s ON s.id_servicio=c.id_servicio
              WHERE s.categoria IS NOT NULL AND s.categoria COLLATE Latin1_General_100_BIN2 <> c.categoria COLLATE Latin1_General_100_BIN2)
        THROW 51115, 'Una categoría existente difiere. No se sobrescribe; revisar manualmente.', 1;
    UPDATE s SET categoria=c.categoria
    FROM dbo.Servicios s JOIN @Clasificaciones c ON c.id_servicio=s.id_servicio
    WHERE s.categoria IS NULL AND s.id_servicio=1;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
-- Admite lotes parciales: 03 no habilita el nuevo contrato hasta clasificar TODOS, incluso inactivos.
SELECT id_servicio,nombre,activo FROM dbo.Servicios WHERE categoria IS NULL ORDER BY id_servicio;
-- Comprobar después: precio y activo deben conservar sus valores previos.
SELECT id_servicio,nombre,descripcion,precio,activo,categoria
FROM dbo.Servicios WHERE id_servicio=1;

GO
