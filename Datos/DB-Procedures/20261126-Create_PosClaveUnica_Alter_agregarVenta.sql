-- POS sin conexion (2026-11-26, ver docs/DECISIONS.md "POS sin conexion: ventas en cola que se suben solas").
-- Espejo SQL Server de DatosPostgres/DB-Migrations/20261126-Create_posclaveunica.sql (ver ahi el detalle de cada columna).
-- Diferencias deliberadas: sin RLS (idEmpresa explicito en cada consulta), camelCase, BIT en vez de boolean,
-- UNIQUEIDENTIFIER en vez de uuid, SYSDATETIME() de la base.
--
-- Ademas corrige agregarVenta: despues del INSERT devolvia "la ultima venta de la sucursal"
-- (select top 1 ... order by idVenta desc). Si entraban dos ventas juntas en la misma sucursal (por ejemplo, la subida
-- de la cola offline y una venta online) podia devolver el id de la OTRA venta y las lineas quedaban colgadas de ella.
-- Ahora devuelve SCOPE_IDENTITY() (el id insertado por ESTA sentencia, inmune a triggers y a otras sesiones).
-- Firma y resultado iguales: el Web clasico, que usa el mismo SP, no se entera.
-- Cuerpo tomado de sp_helptext en la base local SuperCerdo (identico al snapshot docs/08-relevamiento/snapshot-2026-08-18).
-- ANTES DE CORRERLO EN SM/SL: comparar con sp_helptext 'agregarVenta' de esa base; si difiere, NO correr y avisar.
--
-- Aditivo e idempotente (se puede correr dos veces). Correr en SuperCerdo con un usuario con permiso de DDL.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.PosClaveUnica', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PosClaveUnica (
        id                  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PosClaveUnica PRIMARY KEY,
        idEmpresa           INT NOT NULL,
        clientId            UNIQUEIDENTIFIER NOT NULL,
        tipo                VARCHAR(10) NOT NULL,
        idRegistro          INT NOT NULL,
        origen              VARCHAR(10) NOT NULL,
        idSucursal          INT NOT NULL,
        idUsuario           INT NOT NULL,
        idUsuarioSync       INT NOT NULL,
        fechaLocal          DATETIME NOT NULL,
        recibido            DATETIME NOT NULL CONSTRAINT DF_PosClaveUnica_recibido DEFAULT (SYSDATETIME()),
        llegoTarde          BIT NOT NULL CONSTRAINT DF_PosClaveUnica_llegoTarde DEFAULT (0),
        sinCaja             BIT NOT NULL CONSTRAINT DF_PosClaveUnica_sinCaja DEFAULT (0),
        pendienteFacturar   BIT NOT NULL CONSTRAINT DF_PosClaveUnica_pendienteFacturar DEFAULT (0),
        difPrecio           BIT NOT NULL CONSTRAINT DF_PosClaveUnica_difPrecio DEFAULT (0),
        causaCorte          VARCHAR(20) NULL,
        versionCatalogo     VARCHAR(40) NULL,
        detalle             NVARCHAR(MAX) NULL,
        CONSTRAINT CK_PosClaveUnica_tipo CHECK (tipo IN ('venta', 'caja')),
        CONSTRAINT CK_PosClaveUnica_origen CHECK (origen IN ('online', 'offline'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_PosClaveUnica_Empresa_ClientId' AND object_id = OBJECT_ID('dbo.PosClaveUnica'))
    CREATE UNIQUE INDEX UX_PosClaveUnica_Empresa_ClientId ON dbo.PosClaveUnica (idEmpresa, clientId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PosClaveUnica_Registro' AND object_id = OBJECT_ID('dbo.PosClaveUnica'))
    CREATE INDEX IX_PosClaveUnica_Registro ON dbo.PosClaveUnica (idEmpresa, tipo, idRegistro);
GO

ALTER PROCEDURE [dbo].[agregarVenta]
    @idVenta int = 0,
    @idVendedor int = null,
    @idVentaUltima int = null,
    @fechaVenta datetime,
    @turno nvarchar(50),
    @tipoVenta nvarchar(50) = null,
    @diaFestivo nvarchar(50) = null,
    @observaciones nvarchar(200),
    @idPersona int,
    @idSucursal int,
    @nroRemito nvarchar(50),
    @enCtaCte tinyint = 0,
    @formaPago nvarchar(50),
    @tipoComprobante char = null,
    @cuit nvarchar(50) = null,
    @email nvarchar(50) = null,
    @acumRedondeoKgs float = 0,
    @acumRedondeoImporte float = 0,
    @comisionTarjeta float = 0,
    @pagoMixtoEfectivo float = 0
AS
BEGIN
    SET NOCOUNT ON;

    set @diaFestivo = (select Feriados.feriado from Feriados where @fechaVenta between Feriados.desde and Feriados.hasta)

    insert into Ventas (idVendedor, fechaVenta,idSucursal,turno,diaFestivo,observaciones,idPersona,nroRemito,estado, enCtaCte, formaPago, tipoComprobante, cuit, email, acumRedondeoKgs, acumRedondeoImporte, comisionTarjeta, pagoMixtoEfectivo, creado)
    values (@idVendedor, @fechaVenta,@idSucursal,@turno,@diaFestivo,@observaciones,@idPersona,@nroRemito,'', @enCtaCte, @formaPago, @tipoComprobante, @cuit, @email, @acumRedondeoKgs, @acumRedondeoImporte, @comisionTarjeta, @pagoMixtoEfectivo, SYSDATETIME())

    -- 2026-11-26: el id de ESTE insert (antes: la ultima venta de la sucursal, que con dos ventas simultaneas podia ser otra).
    select CAST(SCOPE_IDENTITY() AS int) AS idVenta
END
GO
