-- Borradores en servidor (recuperacion de formularios/ventas no guardadas) para SQL Server:
-- BorradorGenerico (Compras, Stock, Movimientos, Embutidos-Carga, Embutidos-Rapido) y VentaBorrador
-- (venta en curso del POS), cada una con su tabla de eventos append-only. Espejo de las migraciones
-- Postgres 20260922a-Create_borradorgenerico.sql y 20260921b-Create_ventaborrador_...sql; ver
-- docs/DECISIONS.md "Borradores en SQL Server".
--
-- Solo tablas NUEVAS y aditivas: no toca ninguna tabla ni SP existente. Idempotente (IF NOT EXISTS).
-- Sin sintaxis posterior a SQL Server 2008 (sin JSON, THROW, CREATE OR ALTER, CONCAT, IIF, RLS):
-- corre en ServidorSM / San Lorenzo (2008 RTM). Se ejecuta sobre la base activa (sqlcmd -d SuperCerdo);
-- por eso no lleva USE. SuperCerdo no tiene RLS: el codigo (Datos/BorradorGenerico.cs y VentaBorrador.cs)
-- filtra idEmpresa de forma explicita en cada consulta.
--
-- Fuera de esta etapa (etapa 2): tablas Notificaciones y VentaProductoSinAgregar (campana admin y
-- advertencia de "producto sin agregar"); en SQL Server esas funciones quedan apagadas.
--
-- Rollback: apagar BorradorGenerico:Habilitado / PosBorrador:Habilitado y, si se quiere, DROP TABLE de las
-- 4 tablas (nada mas depende de ellas).

-- ---------------------------------------------------------------------------------------------
-- 1) Borrador generico: una fila por formulario en curso. "Interrumpido" NO es un estado guardado:
--    es ACTIVA con ultimoLatido mas viejo que el umbral. Los timestamps los pone el servidor de base
--    (SYSDATETIME()) desde el codigo, nunca el reloj del navegador.
-- ---------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BorradorGenerico')
BEGIN
    CREATE TABLE [dbo].[BorradorGenerico]
    (
        [id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]       INT NOT NULL,
        [idSucursal]      INT NOT NULL,
        [modulo]          VARCHAR(30) NOT NULL,       -- COMPRA / STOCK / MOVIMIENTO / EMBUTIDO_CARGA / EMBUTIDO_RAPIDO
        [idRegistro]      INT NULL,                   -- id del registro que se edita (NULL = alta nueva)
        [idOperador]      INT NOT NULL,               -- operador real (resuelto por cada modulo)
        [idUsuarioSesion] INT NOT NULL,               -- cuenta con la que se inicio sesion (puede ser la compartida)
        [clientId]        UNIQUEIDENTIFIER NOT NULL,  -- token del formulario generado por el navegador
        [resumen]         NVARCHAR(MAX) NULL,         -- 1 linea armada por el modulo, sin parsear el payload
        [cantLineas]      INT NOT NULL CONSTRAINT [DF_BorradorGenerico_cantLineas] DEFAULT (0),
        [payload]         NVARCHAR(MAX) NULL,         -- JSON del formulario, opaco; se vacia al finalizar/descartar
        [estado]          VARCHAR(12) NOT NULL CONSTRAINT [DF_BorradorGenerico_estado] DEFAULT ('ACTIVA'),
        [idResultado]     INT NULL,                   -- id de lo creado al confirmar (idcompra/idmovimiento/idembutido)
        [creado]          DATETIME2(3) NOT NULL CONSTRAINT [DF_BorradorGenerico_creado] DEFAULT (SYSDATETIME()),
        [ultimoLatido]    DATETIME2(3) NOT NULL CONSTRAINT [DF_BorradorGenerico_ultimoLatido] DEFAULT (SYSDATETIME()),
        [actualizado]     DATETIME2(3) NULL,
        [finalizado]      DATETIME2(3) NULL,
        CONSTRAINT [CK_BorradorGenerico_modulo] CHECK ([modulo] IN ('COMPRA', 'STOCK', 'MOVIMIENTO', 'EMBUTIDO_CARGA', 'EMBUTIDO_RAPIDO')),
        CONSTRAINT [CK_BorradorGenerico_estado] CHECK ([estado] IN ('ACTIVA', 'FINALIZADA', 'DESCARTADA'))
    );

    CREATE UNIQUE INDEX [UX_BorradorGenerico_Empresa_ClientId] ON [dbo].[BorradorGenerico] ([idEmpresa], [clientId]);
    CREATE INDEX [IX_BorradorGenerico_Sucursal_Modulo_Estado] ON [dbo].[BorradorGenerico] ([idEmpresa], [idSucursal], [modulo], [estado], [ultimoLatido]);
    CREATE INDEX [IX_BorradorGenerico_Operador_Modulo_Estado] ON [dbo].[BorradorGenerico] ([idEmpresa], [idOperador], [modulo], [estado]);
END
GO

-- ---------------------------------------------------------------------------------------------
-- 2) Eventos de un borrador generico (append-only por convencion del codigo: solo INSERT/SELECT).
-- ---------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BorradorGenericoEvento')
BEGIN
    CREATE TABLE [dbo].[BorradorGenericoEvento]
    (
        [id]         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]  INT NOT NULL,
        [idBorrador] INT NOT NULL,
        [fecha]      DATETIME2(3) NOT NULL CONSTRAINT [DF_BorradorGenericoEvento_fecha] DEFAULT (SYSDATETIME()),
        [tipo]       VARCHAR(20) NOT NULL,
        [idUsuario]  INT NOT NULL,                    -- quien hizo la accion
        [detalle]    NVARCHAR(MAX) NULL,
        CONSTRAINT [CK_BorradorGenericoEvento_tipo] CHECK ([tipo] IN ('CIERRE_PESTANA', 'LOGOUT', 'RECUPERADA', 'DESCARTADA'))
    );

    CREATE INDEX [IX_BorradorGenericoEvento_Borrador] ON [dbo].[BorradorGenericoEvento] ([idEmpresa], [idBorrador], [fecha]);
    CREATE INDEX [IX_BorradorGenericoEvento_Fecha] ON [dbo].[BorradorGenericoEvento] ([idEmpresa], [fecha]);
END
GO

-- ---------------------------------------------------------------------------------------------
-- 3) Venta en curso del POS: una fila por carrito. REGLA DE DISENO (igual que en Postgres): el
--    borrador NUNCA se escribe en Ventas/LineaVenta; las lineas viven solo en el payload.
-- ---------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'VentaBorrador')
BEGIN
    CREATE TABLE [dbo].[VentaBorrador]
    (
        [id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]       INT NOT NULL,
        [idSucursal]      INT NOT NULL,
        [idOperador]      INT NOT NULL,               -- operador real (ResolverOperadorPOS)
        [idUsuarioSesion] INT NOT NULL,
        [clientId]        UNIQUEIDENTIFIER NOT NULL,
        [posInstanceId]   NVARCHAR(200) NULL,
        [idCierreCaja]    INT NULL,
        [idPersona]       INT NULL,
        [razonSocial]     NVARCHAR(MAX) NULL,
        [cantLineas]      INT NOT NULL CONSTRAINT [DF_VentaBorrador_cantLineas] DEFAULT (0),
        [total]           DECIMAL(14,2) NOT NULL CONSTRAINT [DF_VentaBorrador_total] DEFAULT (0),
        [payload]         NVARCHAR(MAX) NULL,         -- lineas/expendios/forma de pago/cliente (forma de POSDraft)
        [estado]          VARCHAR(12) NOT NULL CONSTRAINT [DF_VentaBorrador_estado] DEFAULT ('ACTIVA'),
        [idVenta]         INT NULL,
        [creado]          DATETIME2(3) NOT NULL CONSTRAINT [DF_VentaBorrador_creado] DEFAULT (SYSDATETIME()),
        [ultimoLatido]    DATETIME2(3) NOT NULL CONSTRAINT [DF_VentaBorrador_ultimoLatido] DEFAULT (SYSDATETIME()),
        [actualizado]     DATETIME2(3) NULL,
        [finalizado]      DATETIME2(3) NULL,
        CONSTRAINT [CK_VentaBorrador_estado] CHECK ([estado] IN ('ACTIVA', 'FINALIZADA', 'DESCARTADA'))
    );

    CREATE UNIQUE INDEX [UX_VentaBorrador_Empresa_ClientId] ON [dbo].[VentaBorrador] ([idEmpresa], [clientId]);
    CREATE INDEX [IX_VentaBorrador_Sucursal_Estado] ON [dbo].[VentaBorrador] ([idEmpresa], [idSucursal], [estado], [ultimoLatido]);
    CREATE INDEX [IX_VentaBorrador_Operador_Estado] ON [dbo].[VentaBorrador] ([idEmpresa], [idOperador], [estado]);
END
GO

-- ---------------------------------------------------------------------------------------------
-- 4) Eventos de una venta en curso (append-only por convencion del codigo).
-- ---------------------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'VentaBorradorEvento')
BEGIN
    CREATE TABLE [dbo].[VentaBorradorEvento]
    (
        [id]         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [idEmpresa]  INT NOT NULL,
        [idBorrador] INT NOT NULL,
        [fecha]      DATETIME2(3) NOT NULL CONSTRAINT [DF_VentaBorradorEvento_fecha] DEFAULT (SYSDATETIME()),
        [tipo]       VARCHAR(20) NOT NULL,
        [idUsuario]  INT NOT NULL,
        [detalle]    NVARCHAR(MAX) NULL,
        CONSTRAINT [CK_VentaBorradorEvento_tipo] CHECK ([tipo] IN ('CIERRE_PESTANA', 'LOGOUT', 'CIERRE_CAJA', 'RECUPERADA', 'DESCARTADA'))
    );

    CREATE INDEX [IX_VentaBorradorEvento_Borrador] ON [dbo].[VentaBorradorEvento] ([idEmpresa], [idBorrador], [fecha]);
    CREATE INDEX [IX_VentaBorradorEvento_Fecha] ON [dbo].[VentaBorradorEvento] ([idEmpresa], [fecha]);
END
GO
