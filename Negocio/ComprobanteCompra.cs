using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Comprobantes del proveedor (factura, nota de credito/debito) de una compra y titular propio de la compra.
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres: en SQL Server
    // NegocioFactory.CrearComprobanteCompra devuelve null y las pantallas no ofrecen nada de esto.
    //
    // Reglas (las puras viven en ValidacionComprobanteCompra):
    //  - Los comprobantes NO generan movimientos de cuenta corriente: la deuda es el importe de la compra.
    //  - Guardar o borrar un comprobante recalcula el estado de factura de la compra (PENDIENTE / FACTURADA / NO_LLEVA).
    //  - Todos los comprobantes de una compra son del titular de la compra (una nota de credito va al CUIT de la factura).
    // Cada metodo devuelve null si salio bien o el mensaje para el usuario (los errores de uso no son excepciones).
    // Codigo C# 7.3: Negocio tambien compila para net472.
    public class ComprobanteCompra
    {
        private readonly Contratos.IComprobanteCompraRepository oComprobanteD;
        private readonly TitularFiscal oTitularN;
        private readonly int idEmpresa;

        public ComprobanteCompra(Contratos.IComprobanteCompraRepository repositorio, TitularFiscal titularN, int idEmpresa)
        {
            oComprobanteD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oTitularN = titularN;
            this.idEmpresa = idEmpresa;
        }

        // ------------------------------------------------------------------ lectura

        // Compra con sus comprobantes y totales. null si la compra no existe.
        public Entidades.DetalleComprobantesCompra ObtenerDetalle(int idCompra)
        {
            var compra = oComprobanteD.ObtenerCompraLiviana(idCompra);
            if (compra == null) return null;

            var comprobantes = oComprobanteD.ListarPorCompra(idCompra);
            decimal total = oComprobanteD.ObtenerTotalCompra(idCompra, compra.TipoCompra);
            decimal facturado = ValidacionComprobanteCompra.FacturadoNeto(comprobantes);

            return new Entidades.DetalleComprobantesCompra
            {
                IdCompra = compra.IdCompra,
                TipoCompra = compra.TipoCompra,
                IdTitular = compra.IdTitular,
                EstadoFactura = compra.EstadoFactura,
                TotalCompra = total,
                FacturadoNeto = facturado,
                SinFactura = ValidacionComprobanteCompra.SinFactura(total, facturado),
                Comprobantes = comprobantes
            };
        }

        public Dictionary<int, Entidades.InfoFacturaCompra> InfoPorCompras(IEnumerable<int> idsCompra)
        {
            return oComprobanteD.ObtenerInfoPorCompras(idsCompra);
        }

        // ------------------------------------------------------------------ escritura

        // Alta o edicion de un comprobante de la compra c.IdCompra. Valida, evita duplicados y recalcula el estado.
        public string Guardar(Entidades.ComprobanteCompra c, int idUsuario, out int idComprobante)
        {
            idComprobante = 0;
            if (c == null || c.IdCompra <= 0) return "No se indicó la compra.";

            var compra = oComprobanteD.ObtenerCompraLiviana(c.IdCompra);
            if (compra == null) return "La compra no existe.";
            if (string.Equals(compra.Estado, "Anulado", StringComparison.OrdinalIgnoreCase))
                return "La compra está anulada: no se le pueden cargar comprobantes.";

            // El proveedor y el numero se toman de la compra: el formulario no puede cambiarlos.
            c.IdProveedor = compra.Proveedor.idPersona;

            var activos = oComprobanteD.ListarPorCompra(c.IdCompra);
            var otros = activos.Where(x => x.IdComprobante != c.IdComprobante).ToList();

            if (c.IdComprobante > 0 && activos.All(x => x.IdComprobante != c.IdComprobante))
                return "El comprobante ya no existe.";

            string error = ValidacionComprobanteCompra.Validar(c, otros, compra.TipoCompra);
            if (error != null) return error;

            if (oComprobanteD.ExisteNumero(c.IdProveedor, c.TipoComprobante, c.PtoVta, c.Numero, c.IdComprobante))
                return "Ese comprobante (" + c.TipoComprobante + " " + c.NumeroCompleto + ") ya está cargado para este proveedor.";

            idComprobante = oComprobanteD.Guardar(c, idUsuario);

            RecalcularEstado(c.IdCompra, compra.EstadoFactura, idUsuario);
            return null;
        }

        // Baja logica de un comprobante, con motivo. No se puede borrar la factura si quedan notas.
        public string Eliminar(int idComprobante, int idUsuario, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo)) return "Indicá el motivo de la baja.";

            var comprobante = oComprobanteD.Obtener(idComprobante);
            if (comprobante == null || comprobante.Eliminado) return "El comprobante no existe.";

            var activos = oComprobanteD.ListarPorCompra(comprobante.IdCompra);
            string error = ValidacionComprobanteCompra.ValidarBaja(comprobante, activos);
            if (error != null) return error;

            var compra = oComprobanteD.ObtenerCompraLiviana(comprobante.IdCompra);
            oComprobanteD.Eliminar(idComprobante, idUsuario, motivo.Trim());
            if (compra != null) RecalcularEstado(comprobante.IdCompra, compra.EstadoFactura, idUsuario);
            return null;
        }

        // "Esta compra no lleva factura": solo si no tiene comprobantes activos y es una compra que admite factura.
        public string MarcarSinFactura(int idCompra, int idUsuario)
        {
            var compra = oComprobanteD.ObtenerCompraLiviana(idCompra);
            if (compra == null) return "La compra no existe.";
            if (!ValidacionComprobanteCompra.TipoCompraAdmiteComprobantes(compra.TipoCompra))
                return "Las operaciones de stock no llevan factura.";
            if (oComprobanteD.ListarPorCompra(idCompra).Count > 0)
                return "La compra ya tiene comprobantes cargados: eliminalos antes de marcarla como sin factura.";

            oComprobanteD.SetEstadoFactura(idCompra, Entidades.EstadoFacturaCompra.NoLleva, idUsuario);
            return null;
        }

        // Vuelve a "pendiente" una compra marcada como sin factura (se equivocaron al marcarla).
        public string VolverAPendiente(int idCompra, int idUsuario)
        {
            var compra = oComprobanteD.ObtenerCompraLiviana(idCompra);
            if (compra == null) return "La compra no existe.";
            if (compra.EstadoFactura != Entidades.EstadoFacturaCompra.NoLleva) return "La compra no está marcada como sin factura.";

            oComprobanteD.SetEstadoFactura(idCompra, Entidades.EstadoFacturaCompra.Pendiente, idUsuario);
            return null;
        }

        // Cambia el titular propio de la compra (y de sus movimientos de cuenta corriente, en el lugar). Si la compra ya tiene
        // comprobantes el cambio pide confirmacion, porque los comprobantes van al CUIT del titular.
        public string CambiarTitular(int idCompra, int? idTitular, int idUsuario, bool confirmado, out bool requiereConfirmacion)
        {
            requiereConfirmacion = false;

            var compra = oComprobanteD.ObtenerCompraLiviana(idCompra);
            if (compra == null) return "La compra no existe.";
            if (compra.IdTitular == idTitular) return null;

            string error = ErrorSiTitularNoDisponible(idTitular);
            if (error != null) return error;

            if (oComprobanteD.ListarPorCompra(idCompra).Count > 0 && !confirmado)
            {
                requiereConfirmacion = true;
                return "La compra ya tiene comprobantes cargados a nombre del titular actual. ¿Cambiar el titular igual?";
            }

            oComprobanteD.SetTitularCompra(idCompra, idTitular, idUsuario);
            return null;
        }

        // El titular tiene que ser de la empresa, estar habilitado y no estar revocado. null = sin titular (principal): siempre vale.
        public string ErrorSiTitularNoDisponible(int? idTitular)
        {
            if (!idTitular.HasValue || idTitular.Value <= 0) return null;
            if (oTitularN == null) return "Esta base no maneja titulares.";

            var titular = oTitularN.Obtener(idTitular.Value);
            if (titular == null || titular.IdEmpresa != idEmpresa) return "El titular elegido no existe.";
            if (titular.Revocado) return "La titularidad de " + titular.NombreCorto + " fue cancelada: no se puede usar.";
            if (!titular.Habilitado) return "El titular " + titular.NombreCorto + " no está habilitado.";
            return null;
        }

        // ------------------------------------------------------------------ privado

        private void RecalcularEstado(int idCompra, string estadoActual, int idUsuario)
        {
            var activos = oComprobanteD.ListarPorCompra(idCompra);
            string nuevo = ValidacionComprobanteCompra.EstadoResultante(activos, estadoActual);
            if (nuevo != estadoActual) oComprobanteD.SetEstadoFactura(idCompra, nuevo, idUsuario);
        }
    }
}
