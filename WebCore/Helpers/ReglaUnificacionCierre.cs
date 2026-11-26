// Regla de "Unificar pesajes" en un Cierre Stock: decide si un pesaje (conteo de un sector) se puede unificar en el cierre
// que se esta editando, y si un pesaje ya unificado se puede tomar desde otro pesaje (no).
// Logica pura, sin base ni HTTP: StockController arma los datos y la usa en el modal y al guardar.
// Por que existe y que se descarto: docs/03-modulos/stock-e-inventario.md y docs/DECISIONS.md (2026-11-26).
using System;

namespace WebCore.Helpers
{
    // Datos del pesaje candidato que la regla necesita. StockController los lee de la fila o de la entidad Compra.
    public sealed class PesajeCandidatoCierre
    {
        public int IdCompra { get; set; }
        public bool EsPesaje { get; set; }
        public string Estado { get; set; } = "";
        public int IdSucursal { get; set; }
        public int? CantMedias { get; set; }
        public float? KgsMedias { get; set; }
        // Tiene un proveedor real (no el «Indefinido» que deja «Solo productos»): con medias en 0 es un pesaje de medias
        // guardado «sin pesar» (datos pendientes), no el conteo de un sector.
        public bool ProveedorReal { get; set; }
        // compras.idPesajeAjustado del pesaje: compra real, pesaje padre o cierre al que ya esta vinculado (columna unica).
        public int? IdVinculado { get; set; }
        // Ajuste de stock generado desde este pesaje (0 = ninguno).
        public int IdAjusteGenerado { get; set; }
        // Cantidad de pesajes vinculados A este (si es destino de otros).
        public int CantidadHijos { get; set; }
    }

    public static class ReglaUnificacionCierre
    {
        // Devuelve null si el pesaje se puede unificar en el cierre idCierre (de la sucursal idSucursalCierre), o el motivo
        // en texto corto para mostrar en el modal / en el error de Guardar. idCierre puede ser 0 (cierre nuevo, sin guardar).
        public static string? MotivoNoUnificable(PesajeCandidatoCierre? pesaje, int idCierre, int idSucursalCierre)
        {
            if (pesaje == null || pesaje.IdCompra <= 0)
                return "No se encontró el pesaje.";

            if (!pesaje.EsPesaje)
                return "No es un pesaje.";

            // Anulado (o cualquier estado que no sea el activo de un pesaje sin medias): no se suma a nada.
            if (string.Equals((pesaje.Estado ?? "").Trim(), "Anulado", StringComparison.OrdinalIgnoreCase))
                return "Está anulado.";

            if (idSucursalCierre > 0 && pesaje.IdSucursal != idSucursalCierre)
                return "Es de otra sucursal.";

            // Pesaje de medias: pertenece al circuito compra -> pesaje -> ajuste; sus cortes salen de medias reses, no de un conteo.
            // «Sin pesar» (medias en 0 con proveedor real) es el mismo caso con los datos todavia pendientes.
            if ((pesaje.CantMedias ?? 0) > 0 || (pesaje.KgsMedias ?? 0f) > 0f || pesaje.ProveedorReal)
                return "Es un pesaje de medias.";

            // El ajuste generado SI cuenta para el stock: unificar el pesaje sumaria sus kilos dos veces.
            if (pesaje.IdAjusteGenerado > 0)
                return "Tiene el ajuste #" + pesaje.IdAjusteGenerado + " generado.";

            // idPesajeAjustado es una sola columna: si ya apunta a otro registro, unificarlo pisaria ese vinculo.
            if (pesaje.IdVinculado.HasValue && pesaje.IdVinculado.Value > 0
                && (idCierre <= 0 || pesaje.IdVinculado.Value != idCierre))
                return "Ya está vinculado al #" + pesaje.IdVinculado.Value + ".";

            // Un pesaje destino de otros arrastraria sus hijos: primero se desvinculan.
            if (pesaje.CantidadHijos > 0)
                return "Tiene pesajes vinculados a él.";

            return null;
        }

        // Vincular pesajes dentro de un Pesaje: uno que ya esta unificado en un Cierre Stock no se puede tomar, porque
        // idPesajeAjustado es una sola columna y se perderia el vinculo con el cierre. idVinculado es el idPesajeAjustado
        // del pesaje candidato; vinculadoEsCierre dice si ese registro es un Cierre Stock. Devuelve null si se puede.
        public static string? MotivoNoVinculableAPesaje(int? idVinculado, bool vinculadoEsCierre)
        {
            if (!vinculadoEsCierre || !idVinculado.HasValue || idVinculado.Value <= 0)
                return null;

            return "Está unificado en el Cierre Stock #" + idVinculado.Value + ".";
        }
    }
}
