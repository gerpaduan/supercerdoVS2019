using System;
using System.Collections.Generic;
using Negocio;

namespace WebCore.Models
{
    // Modelo del historial de precios (F8) cuando el cliente tiene presupuestos (2026-10-04, ver
    // docs/DECISIONS.md): solapas Compras | Presupuestos, compartidas por el POS de Ventas
    // (modo "venta": copiar precio) y el de Expendio (modo "expendio": agregar con precio nuevo).
    // Sin presupuestos, Ventas sigue usando el partial original con List<HistorialPrecioProductoVm>.
    public class HistorialPreciosClienteVm
    {
        public const string ModoVenta = "venta";
        public const string ModoExpendio = "expendio";

        public string Modo { get; set; } = ModoVenta;

        // Ultimo precio por producto de las compras reales del cliente (solapa Compras).
        public List<HistorialPrecioProductoVm> Compras { get; set; } = new List<HistorialPrecioProductoVm>();

        // false = el usuario no tiene permiso de ver las compras del cliente (solo en modo
        // expendio, ver VentasController.HistorialPreciosCliente): la solapa Compras no se ofrece.
        public bool MostrarCompras { get; set; } = true;

        // Presupuestos ya evaluados (Negocio.PresupuestosCliente.Evaluar), del mas reciente al mas viejo.
        public List<PresupuestoCliente> Presupuestos { get; set; } = new List<PresupuestoCliente>();

        // Solapa inicial: Presupuestos si hay alguno vigente y/o de los ultimos 6 meses (ver
        // Negocio.PresupuestosCliente.AbrirEnPresupuestos); si no, Compras.
        public bool AbrirEnPresupuestos { get; set; }

        public bool EsModoExpendio => Modo == ModoExpendio;

        // Texto corto del estado del presupuesto para el encabezado de cada bloque.
        public static string TextoEstado(PresupuestoCliente p)
        {
            switch (p.Estado)
            {
                case EstadoPresupuestoCliente.Futuro:
                    // "Rige desde ..." ya figura en el encabezado: aca solo cuando caducaria.
                    return "Caduca el " + p.CaducaEl.ToString("dd/MM/yyyy");
                case EstadoPresupuestoCliente.Caducado:
                    return "Caducó el " + p.CaducaEl.ToString("dd/MM/yyyy");
                default:
                    return "Vigente hasta el " + p.CaducaEl.ToString("dd/MM/yyyy");
            }
        }

        // Por que un producto de un presupuesto no se puede copiar a una venta. "" si se puede
        // (Aplicable). Es el texto del aviso que ve el usuario al tocar el boton bloqueado.
        public static string MotivoNoAplicable(PresupuestoCliente p, LineaPresupuestoCliente linea)
        {
            switch (linea.Estado)
            {
                case EstadoLineaPresupuestoCliente.Futuro:
                    return "No se puede copiar este precio todavía: el presupuesto N° " + p.IdExpendio +
                           " rige desde el " + p.Vigencia.ToString("dd/MM/yyyy HH:mm") + ".";
                case EstadoLineaPresupuestoCliente.Caducado:
                    return "No se puede copiar este precio: el presupuesto N° " + p.IdExpendio +
                           " caducó el " + p.CaducaEl.ToString("dd/MM/yyyy") + ".";
                case EstadoLineaPresupuestoCliente.Reemplazado:
                    return "No se puede copiar este precio: el presupuesto N° " + linea.ReemplazadoPor +
                           " es más reciente y también incluye este producto.";
                default:
                    return "";
            }
        }
    }
}
