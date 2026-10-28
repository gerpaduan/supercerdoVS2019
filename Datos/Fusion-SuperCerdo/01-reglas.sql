-- Fusion SuperCerdo SM + SL (SQL Server local) -- paso 1: reglas.
-- Crea el esquema [fusion] en la base destino (una copia restaurada de la base de San Martin) con las
-- reglas que dicen, para cada tabla y cada columna que guarda un ID, como se trae la fila de San Lorenzo.
-- Detalle y porque de cada regla: docs/DECISIONS.md "Unificacion SuperCerdo SM + SL" (2026-10-27).
-- Lo corre fusionar.ps1 con sqlcmd -v DST=<destino> SM=<base San Martin> SL=<base San Lorenzo> MAPDB=<fusion vieja> MODO=<replica|corregido>.
:on error exit
SET NOCOUNT ON;
USE [$(DST)];
GO
IF SCHEMA_ID('fusion') IS NULL EXEC('CREATE SCHEMA fusion');
GO
IF OBJECT_ID('fusion.Log') IS NOT NULL DROP TABLE fusion.Log;
IF OBJECT_ID('fusion.Param') IS NOT NULL DROP TABLE fusion.Param;
IF OBJECT_ID('fusion.Espacio') IS NOT NULL DROP TABLE fusion.Espacio;
IF OBJECT_ID('fusion.ReglaTabla') IS NOT NULL DROP TABLE fusion.ReglaTabla;
IF OBJECT_ID('fusion.ReglaRef') IS NOT NULL DROP TABLE fusion.ReglaRef;
IF OBJECT_ID('fusion.Map') IS NOT NULL DROP TABLE fusion.Map;
IF OBJECT_ID('fusion.MovDescartado') IS NOT NULL DROP TABLE fusion.MovDescartado;
GO
-- Bitacora de la corrida: cada paso deja cuantas filas toco.
CREATE TABLE fusion.Log (id int IDENTITY PRIMARY KEY, momento datetime NOT NULL DEFAULT GETDATE(),
    paso varchar(40) NOT NULL, tabla sysname NULL, filas int NULL, detalle nvarchar(max) NULL);

-- Parametros de la corrida.
CREATE TABLE fusion.Param (clave sysname PRIMARY KEY, valor nvarchar(400) NOT NULL);
INSERT INTO fusion.Param VALUES ('SM', '$(SM)'), ('SL', '$(SL)'), ('MAPDB', '$(MAPDB)'), ('MODO', '$(MODO)'),
    -- Los IDs transaccionales de San Lorenzo entran corridos en este valor (San Martin conserva los suyos).
    ('OFFSET', '1000000');
IF '$(MODO)' NOT IN ('replica', 'corregido') RAISERROR('MODO debe ser replica o corregido', 16, 1);

-- Espacios de IDs: que significa un valor de ID que viene de San Lorenzo.
--   offset    : el ID de SL pasa a ID + OFFSET (si es > 0; 0 y negativos son "ninguno"/especiales).
--   mapa      : se traduce con fusion.Map (personas, productos, usuarios, tipos de egreso unidos con SM).
--   identidad : el valor significa lo mismo en las dos bases (sucursal, IVA, catalogos, estados).
-- tablaDestino/pkDestino: a que tabla apunta el ID (lo usa 03-verificar.sql para contar referencias huerfanas).
CREATE TABLE fusion.Espacio (espacio varchar(30) PRIMARY KEY, tipo varchar(10) NOT NULL CHECK (tipo IN ('offset','mapa','identidad')),
    tablaDestino sysname NULL, pkDestino sysname NULL);
INSERT INTO fusion.Espacio VALUES
 ('persona','mapa','Personas','idPersona'), ('corte','mapa','Corte','idCorte'), ('usuario','mapa','Usuarios','id'),
 ('tipoegreso','mapa','TiposEgresoCaja','id'),
 ('venta','offset','Ventas','idVenta'), ('lineaventa','offset','LineaVenta','idLineaVenta'), ('compra','offset','Compras','idCompra'),
 ('pago','offset','Pagos','id'), ('movimiento','offset','Movimiento','idMovimiento'), ('embutido','offset','Embutidos','idEmbutido'),
 ('formula','offset','Formulas','idFormula'), ('expendio','offset','Expendios','idExpendio'), ('factura','offset','FacturaElectronica','id'),
 ('dispositivo','offset','DispositivosSeguros','Id'), ('ctacte','offset','MovCtaCte','id'), ('egreso','offset','EgresosCaja','id'),
 ('auditoriaelim','offset','AuditoriaEliminaciones','id'), ('borrador','offset','BorradorGenerico','id'), ('ventaborrador','offset','VentaBorrador','id'),
 ('idtabla','offset',NULL,NULL),     -- idTabla de MovCtaCte/EgresosCaja: apunta a Ventas/Pagos/Compras (todos offset) segun la columna "tabla"
 ('identidad','identidad',NULL,NULL);

-- Regla por tabla. estrategia:
--   sm           : vale la fila de San Martin; lo de SL no se carga (catalogos y configuracion). Se informan diferencias.
--   offset       : se cargan todas las filas de SL, con la PK + OFFSET.
--   copiar       : se cargan todas las filas de SL con la PK tal cual (sus IDs ya no se pisan con SM).
--   sinpk        : se cargan todas las filas de SL (la tabla no tiene PK identity propia).
--   mapa         : se cargan solo las filas de SL que no se unieron con una de SM (fusion.Map.inserta = 1).
--   movimiento   : como offset, descartando la copia del receptor en los movimientos entre sucursales.
--   porsucursal  : configuracion por sucursal (ver 02-fusion.sql paso "porsucursal").
--   vaciaensl    : se espera vacia en SL; si trae filas la corrida aborta (falta definir la regla).
--   ignorar      : tabla vieja que solo existe en SL y el sistema ya no usa; no se carga (se informa).
CREATE TABLE fusion.ReglaTabla (tabla sysname PRIMARY KEY, estrategia varchar(12) NOT NULL, orden int NOT NULL,
    espacioPk varchar(30) NULL, notas nvarchar(400) NULL);
INSERT INTO fusion.ReglaTabla (tabla, estrategia, orden, espacioPk, notas) VALUES
 -- catalogos y configuracion: manda San Martin
 ('AlicuotasIva','sm',0,NULL,NULL), ('Bancos','sm',0,NULL,NULL), ('Conexiones','sm',0,NULL,NULL), ('Empresas','sm',0,NULL,NULL),
 ('EmpresaParametros','sm',0,NULL,NULL), ('Feriados','sm',0,NULL,NULL), ('Formularios','sm',0,NULL,NULL), ('Iva','sm',0,NULL,NULL),
 ('Licencias','sm',0,NULL,NULL), ('Parametros','sm',0,NULL,NULL), ('Proveedores','sm',0,NULL,NULL), ('Sectores','sm',0,NULL,NULL),
 ('Sucursal','sm',0,NULL,NULL), ('TiposProducto','sm',0,NULL,NULL), ('Claves','sm',0,NULL,NULL), ('VencimientosLicencia','sm',0,NULL,NULL),
 ('CatalogoGlobalProducto','sm',0,NULL,NULL), ('sysdiagrams','sm',0,NULL,NULL),
 ('ConfiguracionWhatsApp','sm',0,NULL,'no existe en SL'), ('WhatsAppEnvios','sm',0,NULL,'no existe en SL'),
 -- entidades unidas con SM (fusion.Map)
 ('Personas','mapa',10,'persona','unidas por CUIT / nombre / decision manual del 17/9'),
 ('Corte','mapa',11,'corte','unidos por codigo'),
 ('Usuarios','mapa',12,'usuario','unidos por nombre de usuario'),
 ('TiposEgresoCaja','mapa',13,'tipoegreso','modo corregido: unidos por nombre; modo replica: por id'),
 ('PermisosUsuarios','mapa',14,NULL,'solo los de usuarios de SL que no existian en SM'),
 -- transaccionales
 ('Ventas','offset',20,'venta',NULL), ('LineaVenta','offset',21,'lineaventa',NULL), ('TemporalLineaVenta','offset',22,'identidad',NULL),
 ('FacturaElectronica','offset',23,'factura',NULL), ('AlicuotaIvaPorFactura','offset',24,'identidad',NULL),
 ('Compras','offset',30,'compra',NULL), ('CortePorCompra','offset',31,'identidad',NULL), ('MediaRes','offset',32,'identidad',NULL),
 ('CorteProveedor','offset',33,'identidad',NULL),
 ('Pagos','offset',40,'pago',NULL), ('MovCtaCte','offset',41,'ctacte',NULL), ('Cheques','offset',42,'identidad',NULL),
 ('EgresosCaja','offset',43,'egreso',NULL),
 ('CierreCaja','copiar',44,NULL,'los IDs ya vienen separados de origen: SL 10.000.001+, SM 20.000.001+'),
 ('Embutidos','offset',50,'embutido',NULL), ('CortePorEmbutido','offset',51,'identidad',NULL),
 ('Formulas','offset',52,'formula',NULL), ('CortePorFormula','offset',53,'identidad',NULL),
 ('ActualizacionCorte','sinpk',54,NULL,NULL),
 ('Expendios','offset',60,'expendio',NULL), ('LineaExpendio','offset',61,'identidad',NULL),
 ('LoginUbicacionLog','offset',70,'identidad',NULL), ('DispositivosSeguros','offset',71,'dispositivo',NULL),
 ('Movimiento','movimiento',80,'movimiento',NULL), ('CortePorMovimiento','movimiento',81,'identidad',NULL),
 ('MovimientoHistorial','movimiento',82,NULL,NULL),
 ('CortePuntoStockSucursal','porsucursal',90,'identidad',NULL), ('StockCorteSucursal','porsucursal',91,NULL,'el sistema ya no la usa; se fusiona igual'),
 ('CorteJerarquiaSucursal','porsucursal',92,'identidad',NULL),
 -- tablas nuevas (sep-oct 2026) hoy vacias en SL: si algun dia traen filas hay que definirles regla
 ('AuditoriaEliminaciones','vaciaensl',0,NULL,NULL), ('AuditoriaEliminacionLineas','vaciaensl',0,NULL,NULL),
 ('AuditoriaFormulas','vaciaensl',0,NULL,NULL), ('AuditoriaPagos','vaciaensl',0,NULL,NULL),
 ('BorradorGenerico','vaciaensl',0,NULL,NULL), ('BorradorGenericoEvento','vaciaensl',0,NULL,NULL),
 ('VentaBorrador','vaciaensl',0,NULL,NULL), ('VentaBorradorEvento','vaciaensl',0,NULL,NULL),
 ('DispositivosSeguros_Solicitudes','vaciaensl',0,NULL,NULL), ('UsuarioPasswordResetTokens','vaciaensl',0,NULL,NULL),
 ('CatalogoGlobalImportacionProductos','vaciaensl',0,NULL,NULL),
 -- solo en SL, en desuso (la fusion del 17/9 tampoco las cargo)
 ('Gastos','ignorar',0,NULL,'tabla vieja de SL; los gastos actuales son EgresosCaja'), ('TipoGasto','ignorar',0,NULL,NULL);

-- Regla por columna que guarda un ID de otra tabla (o un usuario). Toda columna "id*", "*Por", etc.
-- que no este aca hace abortar la corrida (ver guardia en 02-fusion.sql).
CREATE TABLE fusion.ReglaRef (tabla sysname NOT NULL, columna sysname NOT NULL, espacio varchar(30) NOT NULL,
    PRIMARY KEY (tabla, columna));
INSERT INTO fusion.ReglaRef VALUES
 ('Ventas','idVendedor','usuario'), ('Ventas','idPersona','persona'), ('Ventas','creadoPor','usuario'), ('Ventas','actualizadoPor','usuario'),
 ('Ventas','idSucursal','identidad'),
 ('LineaVenta','idVenta','venta'), ('LineaVenta','idCorte','corte'), ('LineaVenta','idLineaVentaAnulado','lineaventa'),
 ('LineaVenta','idAnulado','identidad'),       -- no es un ID: guarda el estado de la linea (0 activa / 1 anulada)
 ('LineaVenta','idAlicuotaIva','identidad'),
 ('TemporalLineaVenta','idCorte','corte'), ('TemporalLineaVenta','idVendedor','usuario'), ('TemporalLineaVenta','idSucursal','identidad'),
 ('FacturaElectronica','idVenta','venta'),
 ('AlicuotaIvaPorFactura','idFacturaElectronica','factura'), ('AlicuotaIvaPorFactura','idIva','identidad'),
 ('Compras','idProveedor','persona'), ('Compras','idPesajeAjustado','compra'), ('Compras','idSucursal','identidad'),
 ('Compras','creadoPor','usuario'), ('Compras','actualizadoPor','usuario'),
 ('CortePorCompra','idCompra','compra'), ('CortePorCompra','idCorte','corte'), ('CortePorCompra','idSucursal','identidad'), ('CortePorCompra','creadoPor','usuario'),
 ('MediaRes','idCompra','compra'), ('MediaRes','idSucursal','identidad'),
 ('CorteProveedor','idCorte','corte'), ('CorteProveedor','idProveedor','persona'),
 ('Pagos','idPersona','persona'), ('Pagos','idSucursal','identidad'), ('Pagos','creadoPor','usuario'), ('Pagos','actualizadoPor','usuario'),
 ('Pagos','eliminadoPor','usuario'),
 ('MovCtaCte','idPersona','persona'), ('MovCtaCte','idTabla','idtabla'), ('MovCtaCte','idSucursal','identidad'),
 ('MovCtaCte','creadoPor','usuario'), ('MovCtaCte','actualizadoPor','usuario'),
 ('Cheques','recibidoDe','persona'), ('Cheques','entregadoA','persona'), ('Cheques','creadoPor','usuario'), ('Cheques','actualizadoPor','usuario'),
 ('EgresosCaja','idTipoEgresoCaja','tipoegreso'), ('EgresosCaja','idCompra','compra'), ('EgresosCaja','idTabla','idtabla'),
 ('EgresosCaja','idSucursal','identidad'), ('EgresosCaja','creadoPor','usuario'), ('EgresosCaja','actualizadoPor','usuario'),
 ('CierreCaja','idSucursal','identidad'),
 ('Embutidos','idCorte','corte'), ('Embutidos','idSucursal','identidad'), ('Embutidos','creadoPor','usuario'), ('Embutidos','actualizadoPor','usuario'),
 ('CortePorEmbutido','idEmbutido','embutido'), ('CortePorEmbutido','idCorte','corte'),
 ('Formulas','idEmbutido','embutido'), ('Formulas','creadoPor','usuario'), ('Formulas','actualizadoPor','usuario'),
 ('CortePorFormula','idFormula','formula'), ('CortePorFormula','idCorte','corte'),
 ('ActualizacionCorte','idCorte','corte'), ('ActualizacionCorte','idCorteMaestro','corte'),
 ('Expendios','idVendedor','usuario'), ('Expendios','idVenta','venta'), ('Expendios','idSucursal','identidad'),
 ('Expendios','identificacionExpendio','identidad'),   -- texto libre, no es un ID
 ('LineaExpendio','idExpendio','expendio'), ('LineaExpendio','idCorte','corte'),
 ('LoginUbicacionLog','IdUsuario','usuario'), ('LoginUbicacionLog','IdSucursal','identidad'), ('LoginUbicacionLog','IdDispositivoSeguro','dispositivo'),
 ('DispositivosSeguros','IdUsuarioCreador','usuario'),
 ('Movimiento','idMovOrigen','movimiento'), ('Movimiento','sucursalOrigen','identidad'), ('Movimiento','sucursalDestino','identidad'),
 ('Movimiento','creadoPor','usuario'), ('Movimiento','actualizadoPor','usuario'),
 ('CortePorMovimiento','idMovimientos','movimiento'), ('CortePorMovimiento','idCorte','corte'),
 ('MovimientoHistorial','idMovimiento','movimiento'), ('MovimientoHistorial','idCorte','corte'),
 ('MovimientoHistorial','idSucOrigen','identidad'), ('MovimientoHistorial','idSucDestino','identidad'), ('MovimientoHistorial','actualizadoPor','usuario'),
 ('CortePuntoStockSucursal','idCorte','corte'), ('CortePuntoStockSucursal','idSucursal','identidad'),
 ('StockCorteSucursal','idCorte','corte'), ('StockCorteSucursal','idSucursal','identidad'),
 ('CorteJerarquiaSucursal','idCorte','corte'), ('CorteJerarquiaSucursal','idSucursal','identidad'),
 ('Personas','idPropietario','persona'), ('Personas','idIva','identidad'),
 ('Personas','identificacion','identidad'),    -- texto libre (DNI/codigo), no es un ID
 ('Corte','idCorteMaestro','corte'), ('Corte','idAlicuotaIva','identidad'), ('Corte','idMarca','identidad'), ('Corte','actualizadoPor','usuario'),
 ('Usuarios','idSucursalUser','identidad'),
 ('PermisosUsuarios','idUsuario','usuario'), ('PermisosUsuarios','idForm','identidad');

-- Traduccion de IDs para los espacios "mapa": idSL -> idFinal. inserta=1 si la fila de SL se carga (no tenia pareja en SM).
CREATE TABLE fusion.Map (espacio varchar(30) NOT NULL, idSL int NOT NULL, idFinal int NOT NULL, motivo varchar(40) NOT NULL,
    inserta bit NOT NULL, PRIMARY KEY (espacio, idSL));

-- Movimientos entre sucursales descartados por ser la copia del que recibe (origen: SL o SM).
CREATE TABLE fusion.MovDescartado (base char(2) NOT NULL, idMovimiento int NOT NULL, idMovOrigen int NOT NULL,
    PRIMARY KEY (base, idMovimiento));

INSERT INTO fusion.Log (paso, detalle) VALUES ('reglas', 'MODO=$(MODO) SL=$(SL) MAPDB=$(MAPDB)');
GO
