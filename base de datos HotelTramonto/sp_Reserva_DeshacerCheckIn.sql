
CREATE OR ALTER PROCEDURE dbo.sp_Reserva_DeshacerCheckIn
    @id_reserva INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @estado_reserva VARCHAR(20);
        DECLARE @fecha_checkout DATETIME;

        -- Bloquear la reserva mientras se realizan las validaciones
        SELECT
            @estado_reserva = estado,
            @fecha_checkout = fecha_checkout
        FROM dbo.Reservas WITH (UPDLOCK, HOLDLOCK)
        WHERE id_reserva = @id_reserva;

        -- Validar existencia
        IF @estado_reserva IS NULL
            THROW 50040, 'La reserva no existe.', 1;

        -- Validar estado
        IF @estado_reserva <> 'CHECKIN'
            THROW 50041, 'Solo se puede deshacer el check-in de una reserva en estado CHECKIN.', 1;

        -- Validar que no tenga checkout
        IF @fecha_checkout IS NOT NULL
            THROW 50042, 'No se puede deshacer el check-in porque la reserva ya tiene fecha de checkout.', 1;

        -- Validar que no tenga consumos de servicios
        IF EXISTS (
            SELECT 1
            FROM dbo.DetalleServicios
            WHERE id_reserva = @id_reserva
        )
            THROW 50043, 'No se puede deshacer el check-in porque la reserva tiene consumos de servicios registrados.', 1;

        -- Volver la reserva al estado CONFIRMADA
        UPDATE dbo.Reservas
        SET
            estado = 'CONFIRMADA',
            fecha_checkin = NULL
        WHERE id_reserva = @id_reserva;

        COMMIT TRANSACTION;

        SELECT
            id_reserva,
            estado,
            fecha_checkin,
            fecha_checkout
        FROM dbo.Reservas
        WHERE id_reserva = @id_reserva;

    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
