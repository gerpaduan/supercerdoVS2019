USE [SuperCerdo]
GO
/****** Object:  StoredProcedure [dbo].[getPorcCortesEnMedias]    Script Date: 01/29/2018 16:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[getPorcCortesEnMedias]
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    SELECT    TablaUnion.idCorte, TablaUnion.codigo as 'Codigo', TablaUnion.corte as Corte, TablaUnion.CantKg, TablaUnion.PromPorMedia, TablaUnion.PorcReal, TablaUnion.PorcTeo, TablaUnion.Dif as 'Dif.', TablaUnion.Espacio as '-', TablaUnion.precioKg, TablaUnion.Gan as 'Gan.'
    FROM
	((SELECT    dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte as Corte, sum(dbo.CortePorCompra.cantKg) as CantKg, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.cantMedias) as PromPorMedia, (sum(dbo.CortePorCompra.cantKg) / dbo.Compras.kgsMedias) as PorcReal, (dbo.Corte.porcentaje / 100) as PorcTeo, (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as CantKgTeo, sum(dbo.CortePorCompra.cantKg) - (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100)) as 'Dif', '' as 'Espacio', dbo.Corte.precioKg as 'PrecioKg', (sum(dbo.CortePorCompra.cantKg) - (dbo.Compras.kgsMedias * (dbo.Corte.porcentaje / 100))) * dbo.Corte.precioKg as 'Gan'
	FROM         dbo.Corte INNER JOIN
						  dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
						  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)
	Group by dbo.Compras.idCompra, dbo.Compras.cantMedias, dbo.Compras.kgsMedias, dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.Corte.porcentaje, dbo.Corte.precioKg
	)
	
	UNION
	--se pone el codigo '99999' para que quede ultima la fila de los totales order by  TablaUnion.codigo**
	(SELECT    null as 'idCorte', '99999' as codigo, '' as corte, null as CantKg, null as PromPorMedia,null as PorcReal, null as PorcTeo, null as CantKgTeo, null as 'Dif', null as 'Espacio', null as 'PrecioKg', 0 as 'Gan'
	FROM         dbo.Corte INNER JOIN
						  dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
						  dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)
	)) as TablaUnion
	order by  TablaUnion.codigo

END
