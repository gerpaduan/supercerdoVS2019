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
CREATE PROCEDURE getPromMedias
	-- Add the parameters for the stored procedure here
	@id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  TOP 1   dbo.Compras.cantMedias as CantMedias, dbo.Compras.kgsMedias as Kgs, (dbo.Compras.kgsMedias / dbo.Compras.cantMedias) as PromMedias
FROM         dbo.Corte INNER JOIN
                      dbo.CortePorCompra ON dbo.Corte.idCorte = dbo.CortePorCompra.idCorte INNER JOIN
                      dbo.Compras ON dbo.CortePorCompra.idCompra = dbo.Compras.idCompra
	WHERE     (dbo.CortePorCompra.idCompra = @id)

END
GO
