ALTER PROCEDURE dbo.sp_DetalleServicio_Insertar
    @id_servicios INT,
    @id_reserva INT,
    @cantidad INT,
    @precio_unitario DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DetalleServicios
    (
        id_servicios,
        id_reserva,
        cantidad,
        precio_unitario
    )
    VALUES
    (
        @id_servicios,
        @id_reserva,
        @cantidad,
        @precio_unitario
    );
END;