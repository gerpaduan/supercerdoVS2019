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
Create PROCEDURE getPorcCortesEnMedias
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT    dbo.Corte.codigo as 'Cod.', dbo.Corte.corte as Corte, sum(dbo.CortePorCompra.cantKg) as CantKg, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.cantMedias) as PromPorMedia, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.kgsMedias) as PorcReal, (dbo.Corte.porcentaje / 100) as PorcTeo, (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as CantKgTeo, sum(dbo.CortePorCompra.cantKg) - (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as 'Dif.'
	FROM         dbo.Corte INNER JOIN
						  dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
						  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)
	Group by dbo.Compras.idCompra, dbo.Compras.cantMedias, dbo.Compras.kgsMedias, dbo.Corte.codigo, dbo.Corte.corte, dbo.Corte.porcentaje
	order by  dbo.Corte.codigo

END
GO
