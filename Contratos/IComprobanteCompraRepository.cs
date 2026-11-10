using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Comprobantes del proveedor por compra (factura, nota de credito/debito) y titular propio de la compra.
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Solo Postgres: en SQL Server
    // NegocioFactory.CrearComprobanteCompra devuelve null y las pantallas no muestran nada de esto.
    public interface IComprobanteCompraRepository
    {
        // Datos livianos de la compra que hacen falta para validar: proveedor, tipo, titular, estado de factura, total y fecha.
        // null si la compra no existe (o no es de la empresa).
        Entidades.Compra ObtenerCompraLiviana(int idCompra);
        decimal ObtenerTotalCompra(int idCompra, string tipoCompra);

        List<Entidades.ComprobanteCompra> ListarPorCompra(int idCompra);
        Entidades.ComprobanteCompra Obtener(int idComprobante);
        // Alta (IdComprobante = 0) o edicion. Devuelve el id.
        int Guardar(Entidades.ComprobanteCompra comprobante, int idUsuario);
        // Baja logica con motivo.
        void Eliminar(int idComprobante, int idUsuario, string motivo);
        // true si ya hay otro comprobante activo igual (proveedor + tipo + punto de venta + numero), salvo el excluido.
        bool ExisteNumero(int idProveedor, string tipo, int ptoVta, long numero, int idComprobanteExcluir);

        void SetEstadoFactura(int idCompra, string estado, int idUsuario);
        // Cambia el titular de la compra y el de sus movimientos de cuenta corriente, EN EL LUGAR (el importe no cambia).
        void SetTitularCompra(int idCompra, int? idTitular, int idUsuario);

        // Estado de factura y totales de las compras indicadas (para la bandeja de Compras/Index).
        Dictionary<int, Entidades.InfoFacturaCompra> ObtenerInfoPorCompras(IEnumerable<int> idsCompra);
    }

    // Filas crudas para el resumen por titular de la cuenta corriente de un proveedor (solo Postgres).
    public interface IResumenProveedorRepository
    {
        // Todos los movimientos de la persona de cuenta corriente (el saldo anterior se arma con los previos a "desde").
        List<Entidades.MovimientoResumen> ObtenerMovimientos(int idPersona);
        // Pagos a proveedor (aproveedor = true, no eliminados) desde esa fecha.
        List<Entidades.PagoResumen> ObtenerPagos(int idPersona, DateTime desde);
        // Comprobantes activos de las compras no anuladas de la persona desde esa fecha de compra.
        List<Entidades.FacturadoResumen> ObtenerFacturado(int idPersona, DateTime desde);
        // Extracto de la cuenta corriente con el titular de cada movimiento (mismas columnas que ICuentaCorrienteRepository.
        // getCtaCteByIdPersona mas "idTitular"; el saldo acumulado lo arma Negocio). idTitularFiltro > 0 deja solo ese titular, y
        // el saldo anterior tambien se calcula solo con ese titular. Un movimiento sin titular cuenta como del principal.
        DataTable ObtenerExtractoPorTitular(int idPersona, DateTime desde, int idTitularFiltro);
    }
}
