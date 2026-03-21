CREATE DATABASE CajeroAutomatico
GO

USE CajeroAutomatico
GO

/* =========================
   TABLAS BASE
========================= */

CREATE TABLE EstadoCivil(
    IdEstadoCivil SMALLINT PRIMARY KEY,
    Descripcion VARCHAR(50) NOT NULL
);

CREATE TABLE TipoTelefono(
    IdTipoTelefono SMALLINT PRIMARY KEY,
    Descripcion VARCHAR(50) NOT NULL
);

CREATE TABLE Departamento(
    IdDepartamento INT PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL
);

CREATE TABLE Municipio(
    IdMunicipio INT NOT NULL,
    IdDepartamento INT NOT NULL,
    Descripcion VARCHAR(100) NOT NULL,

    CONSTRAINT PK_Municipio 
        PRIMARY KEY(IdMunicipio, IdDepartamento),

    CONSTRAINT FK_Municipio_Departamento
        FOREIGN KEY(IdDepartamento)
        REFERENCES Departamento(IdDepartamento)
);

/* =========================
   PERSONAS
========================= */

CREATE TABLE Persona(
    IdPersona INT PRIMARY KEY,
    PrimerNombre VARCHAR(50) NOT NULL,
    SegundoNombre VARCHAR(50),
    TercerNombre VARCHAR(50),
    PrimerApellido VARCHAR(50) NOT NULL,
    SegundoApellido VARCHAR(50),
    TercerApellido VARCHAR(50),
    FechaNacimiento DATE NOT NULL,
    Genero CHAR(1) NOT NULL,
    IdEstadoCivil SMALLINT NOT NULL,

    CONSTRAINT FK_Persona_EstadoCivil
        FOREIGN KEY(IdEstadoCivil)
        REFERENCES EstadoCivil(IdEstadoCivil),

    CONSTRAINT CK_Persona_Genero
        CHECK (Genero IN ('H','M'))
);

CREATE TABLE PersonaTelefono(
    IdPersonaTelefono INT NOT NULL,
    IdPersona INT NOT NULL,
    IdTipoTelefono SMALLINT NOT NULL,
    Telefono VARCHAR(20) NOT NULL,

    CONSTRAINT PK_PersonaTelefono
        PRIMARY KEY(IdPersonaTelefono, IdPersona),

    CONSTRAINT FK_PT_Persona
        FOREIGN KEY(IdPersona)
        REFERENCES Persona(IdPersona),

    CONSTRAINT FK_PT_TipoTelefono
        FOREIGN KEY(IdTipoTelefono)
        REFERENCES TipoTelefono(IdTipoTelefono)
);

/* =========================
   CLIENTES
========================= */

CREATE TABLE Cliente(
    IdCliente INT PRIMARY KEY,
    IdPersona INT NOT NULL UNIQUE,

    CONSTRAINT FK_Cliente_Persona
        FOREIGN KEY(IdPersona)
        REFERENCES Persona(IdPersona)
);

/* =========================
   BANCOS Y DIRECCIONES
========================= */

CREATE TABLE Banco(
    IdBanco INT PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL
);

CREATE TABLE Direccion(
    IdDireccion INT PRIMARY KEY,
    IdCliente INT,
    IdBanco INT,
    Calle VARCHAR(100) NOT NULL,
    Avenida VARCHAR(100) NOT NULL,
    Zona VARCHAR(20) NOT NULL,
    Ciudad VARCHAR(100) NOT NULL,
    IdDepartamento INT NOT NULL,
    IdMunicipio INT NOT NULL,

    CONSTRAINT FK_Direccion_Cliente
        FOREIGN KEY(IdCliente)
        REFERENCES Cliente(IdCliente),

    CONSTRAINT FK_Direccion_Banco
        FOREIGN KEY(IdBanco)
        REFERENCES Banco(IdBanco),

    CONSTRAINT FK_Direccion_Municipio
        FOREIGN KEY(IdMunicipio, IdDepartamento)
        REFERENCES Municipio(IdMunicipio, IdDepartamento),

    CONSTRAINT CK_Direccion_Entidad
        CHECK(
            (IdCliente IS NOT NULL AND IdBanco IS NULL)
            OR
            (IdCliente IS NULL AND IdBanco IS NOT NULL)
        )
);

/* =========================
   CUENTAS
========================= */

CREATE TABLE TipoServicioFinanciero(
    IdTipoServicio SMALLINT PRIMARY KEY,
    Descripcion VARCHAR(50) NOT NULL
);

CREATE TABLE Cuenta(
    NumeroCuenta CHAR(20) NOT NULL,
    IdCliente INT NOT NULL,
    IdTipoServicio SMALLINT NOT NULL,
    Estado CHAR(1) NOT NULL,
    Saldo DECIMAL(18,2) NOT NULL,
    FechaApertura DATETIME NOT NULL,
    FechaCierre DATETIME,

    CONSTRAINT PK_Cuenta
        PRIMARY KEY(NumeroCuenta, IdCliente),

    CONSTRAINT FK_Cuenta_Cliente
        FOREIGN KEY(IdCliente)
        REFERENCES Cliente(IdCliente),

    CONSTRAINT FK_Cuenta_TipoServicioFinanciero
        FOREIGN KEY(IdTipoServicio)
        REFERENCES TipoServicioFinanciero(IdTipoServicio),

    CONSTRAINT CK_Cuenta_Estado
        CHECK(Estado IN ('A','I')),

    CONSTRAINT CK_Cuenta_Saldo
        CHECK(Saldo >= 0),

    CONSTRAINT CK_Cuenta_Fechas
        CHECK(
            FechaCierre IS NULL
            OR
            FechaCierre > FechaApertura
        )
);

/* =========================
   TARJETAS
========================= */

CREATE TABLE Tarjeta(
    NumeroTarjeta CHAR(20) PRIMARY KEY,
    NumeroCuenta CHAR(20) NOT NULL,
    IdCliente INT NOT NULL,
    IdTipoServicio SMALLINT NOT NULL,
    Estado CHAR(1) NOT NULL,
    PIN CHAR(4) NOT NULL,
    CCV CHAR(3) NOT NULL,
    FechaEmision DATE NOT NULL,
    FechaExpiracion DATE NOT NULL,

    CONSTRAINT FK_Tarjeta_Cuenta
        FOREIGN KEY(NumeroCuenta,IdCliente)
        REFERENCES Cuenta(NumeroCuenta,IdCliente),

    CONSTRAINT FK_Tarjeta_TipoServicioFinanciero
        FOREIGN KEY(IdTipoServicio)
        REFERENCES TipoServicioFinanciero(IdTipoServicio),

    CONSTRAINT CK_Tarjeta_Estado
        CHECK(Estado IN ('A','I')),

    CONSTRAINT CK_Tarjeta_Fechas
        CHECK(FechaExpiracion > FechaEmision)
);

/* =========================
   MOVIMIENTOS
========================= */

CREATE TABLE TipoMovimientoCuenta(
    IdTipoMovimiento SMALLINT PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL,
    Naturaleza CHAR(1) NOT NULL,

    CONSTRAINT CK_TipoMovimiento_Naturaleza
        CHECK(Naturaleza IN ('R','A'))
);

-- 1. Deposito (A)
-- 2. Retiro (R)
-- .....
-- 5. cheque

-- WAITFOR DELAY '00:00:05'; -- Simula un retraso de 5 segundos para probar concurrencia


CREATE TABLE MovimientoCuenta(
    IdMovimiento INT PRIMARY KEY,
    IdTipoMovimiento SMALLINT NOT NULL, -- 5. cheque, entonces que haga un check en la tabla y que valide la fechaDocumento que no sea nula
    FechaDocumento DATETIME, -- Solo para movimientos de tipo cheque
    NumeroCuenta CHAR(20) NOT NULL,
    IdCliente INT NOT NULL,
    NumeroTarjeta CHAR(20),
    IdPersona INT,
    UsuarioSistema VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    Monto DECIMAL(18,2) NOT NULL,
    FechaHora DATETIME NOT NULL,

    CONSTRAINT FK_Movimiento_Tipo
        FOREIGN KEY(IdTipoMovimiento)
        REFERENCES TipoMovimientoCuenta(IdTipoMovimiento),

    CONSTRAINT CK_Usuario_Sistema
        CHECK(UsuarioSistema = SYSTEM_USER),

    CONSTRAINT CK_Movimiento_Cheque
        CHECK(
            (IdTipoMovimiento = 5 AND FechaDocumento IS NOT NULL AND IdPersona IS NOT NULL)
            OR
            (IdTipoMovimiento != 5 AND FechaDocumento IS NULL)
        ),

    CONSTRAINT FK_Movimiento_Cuenta
        FOREIGN KEY(NumeroCuenta,IdCliente)
        REFERENCES Cuenta(NumeroCuenta,IdCliente),

    CONSTRAINT FK_Movimiento_Tarjeta
        FOREIGN KEY(NumeroTarjeta)
        REFERENCES Tarjeta(NumeroTarjeta),

    CONSTRAINT FK_Movimiento_Persona
        FOREIGN KEY(IdPersona)
        REFERENCES Persona(IdPersona),

    CONSTRAINT CK_Movimiento_Monto
        CHECK(Monto > 0)
);

--select SYSTEM_USER as UsuarioActual;

/* =========================
   BITACORA
========================= */
select * from Cuenta
select * from BitacoraCuenta
select * from MovimientoCuenta
select * from Tarjeta

select * from MovimientoCuenta where monto = 321.47
select * from BitacoraCuenta where IdMovimiento = 20761
go
CREATE TABLE BitacoraCuenta(
    IdBitacora INT NOT NULL,
    IdMovimiento INT NOT NULL,
    Estado CHAR(1) NOT NULL,

    CONSTRAINT PK_Bitacora
        PRIMARY KEY(IdBitacora, IdMovimiento),

    CONSTRAINT FK_Bitacora_Movimiento
        FOREIGN KEY(IdMovimiento)
        REFERENCES MovimientoCuenta(IdMovimiento),

    CONSTRAINT CK_Bitacora_Estado
        CHECK(Estado IN ('I','X','E'))
);

INSERT INTO EstadoCivil VALUES
(1,'Soltero(a)'),
(2,'Casado(a)'),
(3,'Divorciado(a)'),
(4,'Viudo(a)');

INSERT INTO TipoTelefono VALUES
(1,'Celular'),
(2,'Casa'),
(3,'Trabajo');

INSERT INTO Departamento VALUES
(1,'Guatemala'),
(2,'Sacatepequez'),
(3,'Escuintla');

INSERT INTO Municipio VALUES
(1,1,'Guatemala'),
(2,1,'Mixco'),
(3,1,'Villa Nueva'),
(1,2,'Antigua Guatemala'),
(1,3,'Escuintla');

INSERT INTO Persona VALUES
(1,'Carlos','Antonio',NULL,'Ramirez','Lopez',NULL,'1990-05-10','H',1),
(2,'Maria','Fernanda',NULL,'Gomez','Perez',NULL,'1995-08-21','M',2),
(3,'Luis',NULL,NULL,'Martinez','Garcia',NULL,'1988-11-03','H',1),
(4,'Ana','Lucia',NULL,'Hernandez','Ruiz',NULL,'1992-02-14','M',2);

INSERT INTO PersonaTelefono VALUES
(1,1,1,'55510001'),
(2,1,2,'22223333'),
(3,2,1,'55520002'),
(4,3,1,'55530003'),
(5,4,1,'55540004');

INSERT INTO Cliente VALUES
(1,1),
(2,2),
(3,3);

INSERT INTO Banco VALUES
(1,'Banco Central'),
(2,'Banco Nacional');

INSERT INTO Direccion VALUES
(1,1,NULL,'10','5','1','Guatemala',1,1),
(2,2,NULL,'12','7','4','Guatemala',1,2),
(3,3,NULL,'8','3','2','Villa Nueva',1,3),
(4,NULL,1,'1','1','1','Guatemala',1,1);

INSERT INTO TipoServicioFinanciero VALUES
(1,'Monetaria'),
(2,'Ahorro'),
(3,'Credito'),
(4,'Debito');

INSERT INTO Cuenta VALUES
('0000000001',1,1,'A',5000,'2024-01-01',NULL),
('0000000002',2,2,'A',3000,'2024-02-10',NULL),
('0000000003',3,1,'A',1500,'2024-03-15',NULL);

INSERT INTO Tarjeta VALUES
('1111222233334444','0000000001',1,4,'A','1234', '246', '2024-01-01','2028-01-01'),
('5555666677778888','0000000002',2,4,'A','5678', '678', '2024-02-10','2028-02-10');

INSERT INTO TipoMovimientoCuenta VALUES
(1,'Deposito','A'),
(2,'Retiro','R'),
(3,'Nota de credito','A'),
(4,'Nota de debito','R'),
(5,'Pago de cheque','R');

CREATE SEQUENCE SeqMovimiento
    START WITH 1
    INCREMENT BY 1
GO
