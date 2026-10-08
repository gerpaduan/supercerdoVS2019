// Arma la respuesta JSON de "este cambio afecta una caja ya cerrada, confirmalo" que comparten ventas, compras,
// pagos/cobros y egresos de caja. El cliente (wwwroot/Scripts/app/caja-cerrada-confirm.js) la reconoce por
// requiereConfirmacion + tipoConfirmacion = "cajaCerrada" y, si el usuario confirma, reenvia el mismo formulario con
// confirmarCajaCerrada = true. Mismo patron que CerrarCaja (confirmarVentaEnCurso). Ver docs/DECISIONS.md (2026-10-06).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entidades;

namespace WebCore.Helpers
{
    public static class CajaCerradaRespuesta
    {
        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // `cambio`: lineas de texto ya redactadas que describen QUE se esta cambiando (ej. "Monto: $ 100,00 -> $ 150,00").
        // `accion`: "modificar" | "cargar" | "eliminar" (cambia el texto del aviso).
        public static object Confirmacion(IEnumerable<CierreCaja> cajas, IEnumerable<string> cambio, string accion = "modificar", string notaExtra = null)
        {
            var lista = (cajas ?? Enumerable.Empty<CierreCaja>()).ToList();

            string verbo = accion == "eliminar" ? "eliminar este registro" : accion == "cargar" ? "cargar este registro con esa fecha" : "modificar este registro";
            string mensaje = lista.Count <= 1
                ? "Vas a " + verbo + " y pertenece a una caja que ya fue cerrada. El cierre de esa caja va a quedar inconsistente."
                : "Vas a " + verbo + " y afecta a " + lista.Count + " cajas que ya fueron cerradas. Los cierres de esas cajas van a quedar inconsistentes.";

            return new
            {
                ok = false,
                requiereConfirmacion = true,
                tipoConfirmacion = "cajaCerrada",
                mensaje,
                nota = notaExtra ?? "",
                cambio = (cambio ?? Enumerable.Empty<string>()).ToList(),
                cajas = lista.Select(c => new
                {
                    id = c.Id,
                    cajero = c.UsuarioInicio != null ? (c.UsuarioInicio.Nombre ?? "") : "",
                    sucursal = c.Sucursal != null ? (c.Sucursal.SucursalNombre ?? "") : "",
                    desde = c.FechaHoraInicio.HasValue ? c.FechaHoraInicio.Value.ToString("dd/MM/yyyy HH:mm", CulturaAr) : "",
                    hasta = c.FechaHoraCierre.HasValue ? c.FechaHoraCierre.Value.ToString("dd/MM/yyyy HH:mm", CulturaAr) : "",
                    cerradaPor = c.UsuarioCierre != null ? (c.UsuarioCierre.Nombre ?? "") : ""
                }).ToList()
            };
        }

        // "Monto: $ 100,00 -> $ 150,00" (solo si cambia).
        public static string LineaImporte(string rotulo, double anterior, double nuevo)
        {
            return rotulo + ": $ " + anterior.ToString("N2", CulturaAr) + " → $ " + nuevo.ToString("N2", CulturaAr);
        }

        public static string LineaTexto(string rotulo, string anterior, string nuevo)
        {
            return rotulo + ": " + (string.IsNullOrWhiteSpace(anterior) ? "(vacío)" : anterior) + " → " + (string.IsNullOrWhiteSpace(nuevo) ? "(vacío)" : nuevo);
        }

        public static string LineaFecha(string rotulo, DateTime? anterior, DateTime? nuevo)
        {
            string f(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy HH:mm", CulturaAr) : "(sin fecha)";
            return rotulo + ": " + f(anterior) + " → " + f(nuevo);
        }
    }
}
