-- Conteo de billetes como campo propio en pagos/cobros y egresos de caja (2026-10-05, ver docs/DECISIONS.md).
-- Espejo SQL Server de DatosPostgres/DB-Migrations/20261005-Alter_pagos_egresoscaja_add_conteobilletes.sql.
--   dbo.Pagos.ConteoBilletes / dbo.EgresosCaja.ConteoBilletes: texto del conteo de efectivo hecho con el
--   contador de billetes de WebCore. Antes ese texto se mezclaba dentro de observaciones / detalle.
-- Aditivo y nullable. Los parametros nuevos de addOrEditPago / addOrEditEgresoCaja tienen default NULL =
-- "no tocar el conteo": WinForms y el Web clasico (que no mandan @conteoBilletes) al EDITAR conservan el
-- valor que tenia el registro; al insertar sin el parametro queda NULL. Para borrar el conteo WebCore manda
-- '' (cadena vacia), que SI pisa.
-- Cuerpo de ambos SP tomado de docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql (no hay scripts
-- versionados posteriores en esta carpeta). PENDIENTE: verificar con sp_helptext contra la base real antes
-- de aplicar, por si el SP fue tocado a mano despues; pedir aprobacion explicita por servidor.
-- ORDEN DE DEPLOY: este script ANTES de publicar el codigo nuevo.

-- SIN "USE": el script corre sobre la base activa (sqlcmd -d SuperCerdo ..., o la base elegida en SSMS).
-- Antes tenia "USE [carnisys]": en SM y San Lorenzo (base SuperCerdo, WinForms en produccion) aplicaba
-- los cambios en OTRA base o fallaba. Guarda: solo sigue si la base activa tiene Pagos, EgresosCaja y los
-- addOrEditPago / addOrEditEgresoCaja conocidos (INSERT/UPDATE directos sobre dbo.Pagos / dbo.EgresosCaja,
-- sin SESSION_CONTEXT). Si no, avisa y salta TODO lo que sigue (SET NOEXEC ON) sin modificar nada.
-- Es re-ejecutable.
IF OBJECT_ID('dbo.Pagos', 'U') IS NULL
   OR OBJECT_ID('dbo.EgresosCaja', 'U') IS NULL
   OR OBJECT_ID('dbo.addOrEditPago', 'P') IS NULL
   OR OBJECT_ID('dbo.addOrEditEgresoCaja', 'P') IS NULL
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditPago')), '') NOT LIKE '%INSERT INTO dbo.Pagos%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditPago')), '') NOT LIKE '%UPDATE dbo.Pagos%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditPago')), '') LIKE '%SESSION_CONTEXT%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditEgresoCaja')), '') NOT LIKE '%insert into dbo.EgresosCaja%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditEgresoCaja')), '') NOT LIKE '%TiposEgresoCaja%'
   OR ISNULL(OBJECT_DEFINITION(OBJECT_ID('dbo.addOrEditEgresoCaja')), '') LIKE '%SESSION_CONTEXT%'
BEGIN
    RAISERROR('Base equivocada o addOrEditPago / addOrEditEgresoCaja distintos de lo esperado: no se modifico nada. Correr con sqlcmd -d <base> y revisar sp_helptext de ambos.', 16, 1);
    SET NOEXEC ON;
END
GO

IF COL_LENGTH('dbo.Pagos', 'ConteoBilletes') IS NULL
BEGIN
    ALTER TABLE dbo.Pagos ADD ConteoBilletes NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH('dbo.EgresosCaja', 'ConteoBilletes') IS NULL
BEGIN
    ALTER TABLE dbo.EgresosCaja ADD ConteoBilletes NVARCHAR(MAX) NULL;
END
GO

ALTER PROCEDURE [dbo].[addOrEditPago]
		@id int = null,
		@nroRecibo nvarchar(50) = null,
		@fecha datetime = null,
		@idPersona int = null,
		@aProveedor tinyint = null,
		@formaPago nvarchar(50) = null,
		@banco nvarchar(50) = null,
		@nroCheque nvarchar(50) = null,
		@titularCheque nvarchar(50) = null,
		@importe float = null,
		@efectivo float = null,
		@observaciones nvarchar(MAX) = null,
		@idSucursal int = null,
		@creado datetime = null,
		@creadoPor int = null,
		@actualizado datetime = null,
		@actualizadoPor int = null,
		@conteoBilletes nvarchar(MAX) = null
AS
BEGIN
	SET NOCOUNT ON;

		IF @id = 0
			BEGIN
				INSERT INTO dbo.Pagos (nroRecibo, fecha, idPersona, aProveedor, formaPago,
				banco, nroCheque, titularCheque, importe, efectivo, observaciones, idSucursal, creado,
				creadoPor, ConteoBilletes)
				 VALUES (@nroRecibo, @fecha, @idPersona, @aProveedor, @formaPago, @banco, @nroCheque,
				 @titularCheque, @importe, @efectivo, @observaciones, @idSucursal, SYSDATETIME(), @creadoPor, @conteoBilletes)
			set @id = SCOPE_IDENTITY()
			END
		ELSE
		BEGIN
			UPDATE dbo.Pagos set nroRecibo = @nroRecibo, fecha = @fecha, idPersona = @idPersona,
			aProveedor = @aProveedor, formaPago = @formaPago, banco = @banco, nroCheque = @nroCheque,
			titularCheque = @titularCheque, importe = @importe, efectivo = @efectivo, observaciones = @observaciones,
			idSucursal =  @idSucursal, actualizado =  SYSDATETIME(), actualizadoPor =  @actualizadoPor,
			ConteoBilletes = ISNULL(@conteoBilletes, ConteoBilletes)
			WHERE id = @id
		END
	select @id
END
GO

ALTER PROCEDURE [dbo].[addOrEditEgresoCaja]
	@id int = 0,
	@fecha datetime = null,
	@idTipoEgresoCaja int,
	@descripcion nvarchar(MAX),
	@detalle nvarchar(MAX),
	@monto float,
	@idCompra int = null,
	@tabla nvarchar(50) = null,
	@idTabla int = null,
	@esGasto tinyint = null,
	@idSucursal int,
	@creadoPor int = null,
	@actualizadoPor int = null,
	@conteoBilletes nvarchar(MAX) = null
AS
BEGIN
	SET NOCOUNT ON;

	IF @id = 0
		BEGIN
			set @esGasto = (select esGasto from TiposEgresoCaja where TiposEgresoCaja.id = @idTipoEgresoCaja)

			insert into dbo.EgresosCaja (fechaHora,idTipoEgresoCaja,descripcion,detalle,monto, idCompra, tabla, idTabla, esGasto, idSucursal,creado,creadoPor, ConteoBilletes)
			values (@fecha,@idTipoEgresoCaja,@descripcion,@detalle,@monto,@idCompra, @tabla, @idTabla, @esGasto, @idSucursal, SYSDATETIME(),@creadoPor, @conteoBilletes)
			set @id = SCOPE_IDENTITY()
		END
	ELSE
		BEGIN
			--Si @esGasto es null
			IF (@id <> 0 and @id <> 1)
				BEGIN
					set @esGasto = (select TiposEgresoCaja.esGasto from TiposEgresoCaja where TiposEgresoCaja.id = @idTipoEgresoCaja)
				END

			update dbo.EgresosCaja
			set  fechaHora = @fecha,idTipoEgresoCaja = @idTipoEgresoCaja,descripcion = @descripcion,
				detalle = @detalle,monto = @monto, idCompra = @idCompra, tabla = @tabla, idTabla = @idTabla, esGasto = @esGasto, idSucursal = @idSucursal,	actualizado = SYSDATETIME(), actualizadoPor = @actualizadoPor,
				ConteoBilletes = ISNULL(@conteoBilletes, ConteoBilletes)
			where id = @id
		END

	select @id
END
GO

-- Deja la sesion usable si la guarda salteo el script (SSMS conserva NOEXEC entre ejecuciones).
SET NOEXEC OFF
GO
