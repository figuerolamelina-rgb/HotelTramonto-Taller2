
USE HotelTramontoDB;
GO

-- =====================================================
-- 1. ROLES
-- =====================================================
CREATE TABLE Roles (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre_rol VARCHAR(30) NOT NULL UNIQUE,
    descripcion VARCHAR(150) NULL,
    activo BIT NOT NULL DEFAULT 1
);
GO

INSERT INTO Roles (nombre_rol, descripcion)
VALUES
('ADMINISTRADOR', 'Configuracion y administracion del sistema'),
('RECEPCIONISTA', 'Gestion de huespedes, reservas, disponibilidad y pagos'),
('GERENTE', 'Consultas, estadisticas y reportes del hotel');
GO

-- =====================================================
-- 2. USUARIOS
-- =====================================================
CREATE TABLE Usuarios (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_rol INT NOT NULL,
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    dni VARCHAR(15) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    contrasena VARCHAR(255) NOT NULL,
    telefono VARCHAR(20) NULL,
    activo BIT NOT NULL DEFAULT 1,
    fecha_alta DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Usuarios_Roles
        FOREIGN KEY (id_rol)
        REFERENCES Roles(id_rol)
);
GO

-- =====================================================
-- 3. HUESPEDES
-- =====================================================
CREATE TABLE Huespedes (
    id_huesped INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    dni VARCHAR(15) NOT NULL UNIQUE,
    telefono VARCHAR(20) NULL,
    email VARCHAR(100) NULL,
    nacionalidad VARCHAR(50) NOT NULL,
    activo BIT NOT NULL DEFAULT 1,
    fecha_alta DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =====================================================
-- 4. TIPOS DE HABITACION
-- =====================================================
CREATE TABLE TiposHabitacion (
    id_tipoHabitacion INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(200) NULL,
    precio_noche DECIMAL(10,2) NOT NULL,
    capacidad INT NOT NULL,
    activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_TiposHabitacion_Precio
        CHECK (precio_noche > 0),

    CONSTRAINT CK_TiposHabitacion_Capacidad
        CHECK (capacidad > 0)
);
GO

-- =====================================================
-- 5. HABITACIONES
-- =====================================================
CREATE TABLE Habitaciones (
    id_habitacion INT IDENTITY(1,1) PRIMARY KEY,
    tipo_habitacion INT NOT NULL,
    piso INT NULL,
    numero VARCHAR(10) NOT NULL UNIQUE,
    estado VARCHAR(20) NOT NULL,
    activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Habitaciones_TiposHabitacion
        FOREIGN KEY (tipo_habitacion)
        REFERENCES TiposHabitacion(id_tipoHabitacion),

    CONSTRAINT CK_Habitaciones_Estado
        CHECK (estado IN (
            'DISPONIBLE',
            'OCUPADA',
            'MANTENIMIENTO'
        ))
);
GO

-- =====================================================
-- 6. SERVICIOS
-- =====================================================
CREATE TABLE Servicios (
    id_servicio INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    descripcion VARCHAR(150) NULL,
    precio DECIMAL(10,2) NOT NULL,
    activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_Servicios_Precio
        CHECK (precio >= 0)
);
GO

-- =====================================================
-- 7. RESERVAS DEFINITIVAS
-- =====================================================
CREATE TABLE Reservas (
    id_reserva INT IDENTITY(1,1) PRIMARY KEY,
    id_huesped INT NOT NULL,
    id_usuario INT NOT NULL,
    fecha_entrada DATE NOT NULL,
    fecha_salida DATE NOT NULL,
    estado VARCHAR(20) NOT NULL,
    total DECIMAL(10,2) NOT NULL,
    modalidad_pago VARCHAR(20) NULL,
    observaciones VARCHAR(250) NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_checkin DATETIME NULL,
    fecha_checkout DATETIME NULL,

    CONSTRAINT FK_Reservas_Huespedes
        FOREIGN KEY (id_huesped)
        REFERENCES Huespedes(id_huesped),

    CONSTRAINT FK_Reservas_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT CK_Reservas_Fechas
        CHECK (fecha_salida > fecha_entrada),

    CONSTRAINT CK_Reservas_Total
        CHECK (total >= 0),

    CONSTRAINT CK_Reservas_ModalidadPago
        CHECK (
            modalidad_pago IS NULL
            OR modalidad_pago IN ('PAGO_TOTAL', 'SENA_50')
        ),

    CONSTRAINT CK_Reservas_Estado
        CHECK (estado IN (
            'CONFIRMADA',
            'CANCELADA',
            'CHECKIN',
            'FINALIZADA'
        ))
);
GO

-- =====================================================
-- 8. DETALLE DE RESERVAS
-- =====================================================
CREATE TABLE DetalleReservas (
    id_detalle_reserva INT IDENTITY(1,1) PRIMARY KEY,
    id_reserva INT NOT NULL,
    id_habitacion INT NOT NULL,
    precio_noche DECIMAL(10,2) NOT NULL,
    cantidad_personas INT NOT NULL,

    CONSTRAINT FK_DetalleReservas_Reservas
        FOREIGN KEY (id_reserva)
        REFERENCES Reservas(id_reserva),

    CONSTRAINT FK_DetalleReservas_Habitaciones
        FOREIGN KEY (id_habitacion)
        REFERENCES Habitaciones(id_habitacion),

    CONSTRAINT CK_DetalleReservas_Precio
        CHECK (precio_noche > 0),

    CONSTRAINT CK_DetalleReservas_CantidadPersonas
        CHECK (cantidad_personas > 0),

    CONSTRAINT UQ_DetalleReservas_ReservaHabitacion
        UNIQUE (id_reserva, id_habitacion)
);
GO

-- =====================================================
-- 9. RESERVAS TEMPORALES
-- =====================================================
CREATE TABLE ReservasTemporales (
    id_reserva_temporal INT IDENTITY(1,1) PRIMARY KEY,
    id_huesped INT NOT NULL,
    id_habitacion INT NOT NULL,
    id_reserva INT NULL,
    modalidad_pago VARCHAR(20) NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'PENDIENTE',
    fecha_entrada DATE NOT NULL,
    fecha_salida DATE NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT GETDATE(),
    fecha_expiracion DATETIME NOT NULL,

    CONSTRAINT FK_ReservasTemporales_Huespedes
        FOREIGN KEY (id_huesped)
        REFERENCES Huespedes(id_huesped),

    CONSTRAINT FK_ReservasTemporales_Habitaciones
        FOREIGN KEY (id_habitacion)
        REFERENCES Habitaciones(id_habitacion),

    CONSTRAINT FK_ReservasTemporales_Reservas
        FOREIGN KEY (id_reserva)
        REFERENCES Reservas(id_reserva),

    CONSTRAINT CK_ReservasTemporales_Fechas
        CHECK (fecha_salida > fecha_entrada),

    CONSTRAINT CK_ReservasTemporales_Expiracion
        CHECK (fecha_expiracion > fecha_creacion),

    CONSTRAINT CK_ReservasTemporales_ModalidadPago
        CHECK (modalidad_pago IN ('PAGO_TOTAL', 'SENA_50')),

    CONSTRAINT CK_ReservasTemporales_Estado
        CHECK (estado IN (
            'PENDIENTE',
            'CONFIRMADA',
            'CANCELADA',
            'EXPIRADA'
        ))
);
GO

-- =====================================================
-- 10. PAGOS
-- =====================================================
CREATE TABLE Pagos (
    id_pago INT IDENTITY(1,1) PRIMARY KEY,
    id_reserva INT NULL,
    id_reserva_temporal INT NULL,
    id_usuario INT NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    metodo_pago VARCHAR(30) NOT NULL,
    estado VARCHAR(20) NOT NULL,
    tipo_pago VARCHAR(30) NOT NULL,
    fecha_pago DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Pagos_Reservas
        FOREIGN KEY (id_reserva)
        REFERENCES Reservas(id_reserva),

    CONSTRAINT FK_Pagos_ReservasTemporales
        FOREIGN KEY (id_reserva_temporal)
        REFERENCES ReservasTemporales(id_reserva_temporal),

    CONSTRAINT FK_Pagos_Usuarios
        FOREIGN KEY (id_usuario)
        REFERENCES Usuarios(id_usuario),

    CONSTRAINT CK_Pagos_Monto
        CHECK (monto > 0),

    CONSTRAINT CK_Pagos_Estado
        CHECK (estado IN (
            'PENDIENTE',
            'APROBADO',
            'RECHAZADO',
            'REEMBOLSADO'
        )),

    CONSTRAINT CK_Pagos_TipoPago
        CHECK (tipo_pago IN (
            'PAGO_INICIAL',
            'SALDO_ALOJAMIENTO',
            'SERVICIOS_ADICIONALES'
        )),

    CONSTRAINT CK_Pagos_Reserva_Unica
        CHECK (
            (id_reserva IS NOT NULL AND id_reserva_temporal IS NULL)
            OR
            (id_reserva IS NULL AND id_reserva_temporal IS NOT NULL)
        )
);
GO

-- =====================================================
-- 11. DETALLE DE SERVICIOS
-- =====================================================
CREATE TABLE DetalleServicios (
    id_detalle_servicio INT IDENTITY(1,1) PRIMARY KEY,
    id_servicios INT NOT NULL,
    id_reserva INT NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_DetalleServicios_Servicios
        FOREIGN KEY (id_servicios)
        REFERENCES Servicios(id_servicio),

    CONSTRAINT FK_DetalleServicios_Reservas
        FOREIGN KEY (id_reserva)
        REFERENCES Reservas(id_reserva),

    CONSTRAINT CK_DetalleServicios_Cantidad
        CHECK (cantidad > 0),

    CONSTRAINT CK_DetalleServicios_PrecioUnitario
        CHECK (precio_unitario >= 0)
);
GO
