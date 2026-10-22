using System;
using System.Collections.Generic;

namespace Negocio
{
    // Reglas puras (sin base de datos) de la eliminacion de Compras, Stock y Movimientos con motivo y clave, mas los
    // objetos que viajan entre Negocio y WebCore. Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
    public static class EliminacionRegistro
    {
        // Cortes y Media Res son compras reales (Proveedor + cta cte); el resto de los tipos guardados en la tabla Compras son
        // operaciones de stock (Ingreso, Egreso, Cierre, Ajuste, Pesaje).
        public static string ModuloDeTipoCompra(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes), StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.OrdinalIgnoreCase)
                ? Entidades.AuditoriaEliminacion.ModuloCompra
                : Entidades.AuditoriaEliminacion.ModuloStock;
        }

        // Ajuste y Cierre de stock mueven el stock de toda la sucursal (el cierre es el punto de partida del calculo): solo admin,
        // igual que ya pasa para crear un Ajuste (StockController.Editar).
        public static bool RequiereAdmin(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock), StringComparison.OrdinalIgnoreCase)
                || string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPesaje(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes), StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsAjuste(string tipoCompra)
        {
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock), StringComparison.OrdinalIgnoreCase);
        }

        // Devuelve null si el motivo es valido (y lo deja recortado en `normalizado`); si no, el mensaje para el usuario.
        public static string ValidarMotivo(string motivo, out string normalizado)
        {
            normalizado = (motivo ?? "").Trim();
            if (normalizado.Length < Entidades.AuditoriaEliminacion.LargoMinimoMotivo)
                return "Escriba el motivo de la eliminación (al menos " + Entidades.AuditoriaEliminacion.LargoMinimoMotivo + " caracteres).";
            if (normalizado.Length > Entidades.AuditoriaEliminacion.LargoMaximoMotivo)
                return "El motivo no puede superar los " + Entidades.AuditoriaEliminacion.LargoMaximoMotivo + " caracteres.";
            return null;
        }

        // "#12, #15 y #20"
        public static string ListarIds(IEnumerable<int> ids)
        {
            var lista = new List<string>();
            foreach (int id in ids) lista.Add("#" + id);
            if (lista.Count <= 1) return string.Join("", lista);
            return string.Join(", ", lista.GetRange(0, lista.Count - 1)) + " y " + lista[lista.Count - 1];
        }
    }

    // Resultado de eliminar. Ok=false con el motivo en Mensaje (nada se escribio); Ok=true puede traer Advertencia (algo
    // secundario que no se pudo hacer, la eliminacion ya quedo hecha).
    public class ResultadoEliminacion
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public string Advertencia { get; set; }
        public int IdAuditoria { get; set; }

        // Auditoria ya guardada (con su Id). La usa el controller para avisar al admin cuando Negocio no lo hizo.
        public Entidades.AuditoriaEliminacion Auditoria { get; set; }

        public static ResultadoEliminacion Fallo(string mensaje) => new ResultadoEliminacion { Ok = false, Mensaje = mensaje };
    }

    // Lo que se le muestra al usuario antes de confirmar: resumen del registro, efectos y bloqueos.
    public class VistaPreviaEliminacion
    {
        public string Modulo { get; set; }
        public int IdRegistro { get; set; }
        public string TipoRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; }
        public int IdSucursalDestino { get; set; }
        public string SucursalDestinoNombre { get; set; }
        public string PersonaNombre { get; set; }
        public string NroRemito { get; set; }
        public int IdUsuarioCreador { get; set; }
        public bool EnCtaCte { get; set; }
        public double Importe { get; set; }
        public double TotalKg { get; set; }
        public int CantLineas { get; set; }
        public bool RequiereAdmin { get; set; }

        // Texto de lo que se hace en la cuenta corriente (null si no aplica).
        public string EfectoCtaCte { get; set; }

        // Egreso de caja de la compra (solo las cargadas desde el POS). El controller evalua si su caja esta cerrada.
        public Entidades.EgresoCaja Egreso { get; set; }

        // Avisos informativos (no bloquean), por ejemplo que el precio de venta ya actualizado no se revierte.
        public List<string> Avisos { get; set; } = new List<string>();

        // Motivos por los que NO se puede eliminar (registros que dependen de este). Vacio = se puede.
        public List<string> Bloqueos { get; set; } = new List<string>();

        public List<Entidades.AuditoriaEliminacionLinea> Lineas { get; set; } = new List<Entidades.AuditoriaEliminacionLinea>();
    }

    // Excepcion interna: el registro lo elimino otro usuario entre la lectura y el borrado. Hace rollback de lo ya escrito.
    internal class RegistroYaEliminadoException : Exception
    {
        public RegistroYaEliminadoException() : base("El registro ya había sido eliminado.") { }
    }
}
