USE [CarniSys]
GO

-- ============================================================================
-- Indice faltante sobre FacturaElectronica.fechaEmisionAfip, la columna del rango
-- de fechas y del ORDER BY de Datos.Venta.BuscarFacturasPagina/ObtenerFacturasResumen
-- (antes: Datos.Venta.getFacturasRealizadas, ver docs/DECISIONS.md). Sin este indice,
-- cualquier consulta de la pantalla /Ventas/Facturas hacia un table scan completo de
-- la tabla sin importar cuan angosto sea el rango de fechas pedido -- irrelevante en
-- la base local (98 filas) pero real en produccion (SM: 22.629 filas / San Lorenzo:
-- 57.184 filas, medido 2026-08-05).
--
-- DESC para matchear el ORDER BY (fechaEmisionAfip DESC, id DESC) y evitar un sort
-- aparte. INCLUDE (CAE, idVenta) para que el filtro "ISNULL(CAE,'')<>''" y el JOIN
-- a Ventas se resuelvan contra el propio indice, sin un lookup extra a la tabla.
-- ============================================================================
CREATE INDEX IX_FacturaElectronica_FechaEmisionAfip
ON dbo.FacturaElectronica (fechaEmisionAfip DESC)
INCLUDE (CAE, idVenta);
GO
