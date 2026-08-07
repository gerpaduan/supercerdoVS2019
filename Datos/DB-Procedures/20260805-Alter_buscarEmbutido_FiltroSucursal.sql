USE [CarniSys]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Versiona (nunca lo estuvo) el fix aplicado directo en la base local el 2026-07-31:
-- el filtro de sucursal era casi siempre verdadero por "or dbo.Embutidos.idSucursal > 0"
-- sin el guard "@idSucursal <= 0", asi que el combo "Sucursal" de /Elaborados/Index nunca
-- filtraba de verdad (devolvia todas las sucursales sin importar la seleccionada).
-- Ver docs/07-operacion-y-soporte/incidencias-frecuentes.md, entrada 2026-07-31.
-- Replica tal cual el ORDER BY actual de la version local (ascendente por
-- fechaEmbutido) -- SM hoy lo tiene descendente; esa diferencia de orden NO se
-- resuelve en este script (no fue parte del pedido), queda anotada como
-- pendiente de confirmar con el usuario cual de los dos ordenes es el correcto.
ALTER PROCEDURE [dbo].[buscarEmbutido]
	@texto nvarchar(50),
	@idSucursal int = 0,
	@fechaDesde datetime,
	@fechaHasta datetime
AS
BEGIN
	SET NOCOUNT ON;

	SELECT     dbo.Embutidos.idEmbutido as 'Id', dbo.Embutidos.fechaEmbutido as 'Fecha',CorteEmbutido.codigo as 'Codigo', CorteEmbutido.corte as 'Corte',
                      SUM(dbo.CortePorEmbutido.kgUtilizados) AS 'Kgs', dbo.Sucursal.sucursal as 'Sucursal', dbo.Embutidos.estado  as 'Estado', 'Observaciones' = case
                      when LEN(dbo.Embutidos.observaciones) <= 20 then dbo.Embutidos.observaciones
                      else (SUBSTRING(dbo.Embutidos.observaciones, 1, 20) + '...') end ,
                      dbo.Embutidos.creado as 'Creado', CreadoPor.nombre AS 'Creado Por',
                      dbo.Embutidos.actualizado as 'Actualizado', ActualizadoPor.nombre AS 'Actualizado Por'
FROM          dbo.Corte INNER JOIN
                      dbo.CortePorEmbutido ON dbo.Corte.idCorte = dbo.CortePorEmbutido.idCorte INNER JOIN
                      dbo.Embutidos ON dbo.CortePorEmbutido.idEmbutido = dbo.Embutidos.idEmbutido INNER JOIN
                      dbo.Corte AS CorteEmbutido ON dbo.Embutidos.idCorte = CorteEmbutido.idCorte INNER JOIN
                      dbo.Sucursal ON dbo.Embutidos.idSucursal = dbo.Sucursal.idSucursal LEFT OUTER JOIN
                      dbo.Usuarios AS CreadoPor ON dbo.Embutidos.creadoPor = CreadoPor.id LEFT OUTER JOIN
                      dbo.Usuarios AS ActualizadoPor ON dbo.Embutidos.actualizadoPor = ActualizadoPor.id
WHERE fechaEmbutido between @fechaDesde and @fechaHasta
and
(
(@idSucursal > 0 and dbo.Sucursal.idSucursal = @idSucursal) or
(@idSucursal <= 0 and dbo.Embutidos.idSucursal > 0)
)
and ((CAST( CorteEmbutido.codigo as nvarchar(50)) = @texto )or(CorteEmbutido.corte like '%'+@texto+'%' ) or (CreadoPor.nombre like '%'+@texto+'%' ) or (ActualizadoPor.nombre like '%'+@texto+'%' ))
GROUP BY dbo.Embutidos.idEmbutido, dbo.Embutidos.fechaEmbutido, CorteEmbutido.codigo, CorteEmbutido.corte, dbo.Sucursal.sucursal, dbo.Embutidos.creado, CreadoPor.nombre, dbo.Embutidos.actualizado, ActualizadoPor.nombre, dbo.Embutidos.estado, dbo.Embutidos.observaciones
ORDER BY fechaEmbutido, dbo.Embutidos.creado DESC

END
GO
