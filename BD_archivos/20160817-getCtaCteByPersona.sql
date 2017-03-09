-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE getCtaCteByIdPersona
	-- Add the parameters for the stored procedure here
	@idPersona int,
	@fechaDesde datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT * FROM
    (
    SELECT     dbo.Personas.idPersona, dbo.Personas.razonSocial, '-' AS id, @fechaDesde AS fecha, '-' AS tabla, '-' AS idTabla, 'Saldo Anterior' AS detalle, '-' AS tipo, SaldoAnteriorTabla.SaldoAnterior AS importe, 0 AS Saldo, '-' AS sucursal, @fechaDesde AS creado, '-' AS  CreadoPor, @fechaDesde AS actualizado, '-' AS ActualizadoPor
	FROM 
    (SELECT     dbo.Personas.idPersona, SUM(dbo.MovCtaCte.importe) AS SaldoAnterior
	FROM         dbo.MovCtaCte INNER JOIN
						  dbo.Personas ON dbo.MovCtaCte.idPersona = dbo.Personas.idPersona 
	Where dbo.Personas.idPersona = @idPersona and dbo.MovCtaCte.fecha < @fechaDesde
	GROUP BY dbo.Personas.idPersona) as SaldoAnteriorTabla INNER JOIN
                      dbo.Personas ON SaldoAnteriorTabla.idPersona = dbo.Personas.idPersona 	
    union
    
	SELECT     dbo.Personas.idPersona, dbo.Personas.razonSocial, dbo.MovCtaCte.id, dbo.MovCtaCte.fecha, dbo.MovCtaCte.tabla, dbo.MovCtaCte.idTabla, dbo.MovCtaCte.detalle, dbo.MovCtaCte.tipo, 
					  dbo.MovCtaCte.importe, 0.00 AS Saldo, dbo.Sucursal.sucursal, dbo.MovCtaCte.creado, CreadoPor.nombre AS CreadoPor, dbo.MovCtaCte.actualizado, ActualizadoPor.nombre AS ActualizadoPor
	FROM         dbo.MovCtaCte INNER JOIN
                      dbo.Personas ON dbo.MovCtaCte.idPersona = dbo.Personas.idPersona LEFT OUTER JOIN
                      dbo.Sucursal ON dbo.MovCtaCte.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.MovCtaCte.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.MovCtaCte.actualizadoPor = ActualizadoPor.id
	Where dbo.Personas.idPersona = @idPersona and dbo.MovCtaCte.fecha > @fechaDesde
	) as MovCtaCte
	Order by fecha
END
GO
