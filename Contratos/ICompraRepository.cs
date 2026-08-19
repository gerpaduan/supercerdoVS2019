using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja los 30 metodos reales de Datos.Compra (Compras, CortePorCompra, MediaRes).
    // backup/restaurarBD (BACKUP DATABASE/RESTORE DATABASE de SQL Server, sin equivalente en
    // Postgres) quedan fuera de esta interfaz -- no es un olvido. Ver docs/DECISIONS.md, Etapa 9.
    public interface ICompraRepository
    {
        void anularCompra(int idCompra);
        DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal);
        DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal);
        DataTable findById(int idCompra);
        int addOrEditCompra(Entidades.Compra oCompraE);
        int agregarCompra(Entidades.Compra oCompraE);
        void ModificarCompra(Entidades.Compra oCompraE);
        void actualizarObservacionesCompra(int idCompra, string observaciones, int actualizadoPor);
        List<int> obtenerPesajesVinculadosPorDestino(int idPesajeDestino);
        Dictionary<int, List<int>> obtenerPesajesVinculadosPorDestinos(IEnumerable<int> idsDestino);
        void actualizarIdPesajeAjustado(int idCompra, int? idPesajeAjustado, int actualizadoPor);
        float getTotalCompra(int idCompra, string tipoCompra);
        void modificarPrecioMedia(int idCompra, float precioKg);
        void agregarCortePorCompra(Entidades.CortePorCompra oCorteE);
        void limpiarCortesPorCompra(int idCompra);
        void agregarMediaRes(Entidades.MediaRes oMediaResE);
        int obtenerIdUltimaCompra();
        DataTable obtenerCortesPorCompra(int idCompra);
        DataTable obtenerMediasPorCompra(int idCompra);
        void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra);
        void modificarCortePorCompra(Entidades.CortePorCompra oCorteE, int idCompra);

        // No-op deliberado: el SP real (quitarStockMedia) solo actualiza StockCorteSucursal
        // (cascada de stock), tabla que nunca se porta a Postgres (Etapa 6). No es un gap.
        void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra);

        // Solo replica la parte real (DELETE FROM MediaRes) -- el resto del SP real
        // (quitarStockTeoricoMedia) es cascada de StockCorteSucursal, no-op.
        void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra);

        void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra);
        DataTable porcentajeCortesPorCompra(int idCompra);
        DataTable getPromMedias(int idCompra);
        DataTable getPorcCortesEnMedias(int idCompra);
        int getIdAjusteDelPesaje(int idPesaje);
        Dictionary<int, int> getIdsAjustePorPesajes(IEnumerable<int> idsPesaje);
        void actualizarEstadoPesaje(int idPesaje, Entidades.Compra.estadoAjusteStock estadoAjStock);
    }
}
