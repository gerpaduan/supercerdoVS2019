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
CREATE PROCEDURE getListaElegirEmbutido
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT     CorteEmbutido.idCorte AS idCorteEmbutido, CorteEmbutido.codigo as codigoEmbutido, 
		CorteEmbutido.corte as corteEmbutido, corteEn.idCorte AS idCorteEn, corteEn.codigo AS codigoEn, 
							  corteEn.corte AS corteEn
		FROM         dbo.Corte as corteEn CROSS JOIN
							  dbo.Corte AS CorteEmbutido
		Where (CorteEmbutido.codigo = 75 and corteEn.codigo = 175)
			OR (CorteEmbutido.codigo = 99 and corteEn.codigo = 199)
			--OR (CorteEmbutido.codigo = 17 and corteEn.codigo = 67)
			OR (CorteEmbutido.codigo = 8 and corteEn.codigo = 14)
			
		Order by CorteEmbutido.codigo;
END
GO
