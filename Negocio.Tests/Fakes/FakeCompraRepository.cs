using System;
using System.Collections.Generic;
using System.Data;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake minimo de ICompraRepository, mismo criterio que FakeVentaRepository: solo
    // IniciarUnitOfWork/addOrEditCompra tienen cuerpo real -- lo unico que un escenario "sin
    // medias, sin cortes, sin egreso de caja" de Negocio.Compra.AddOrEditCompra llama. El resto
    // tira NotImplementedException.
    public sealed class FakeCompraRepository : Contratos.ICompraRepository
    {
        private readonly Contratos.IUnitOfWork _unitOfWorkAEntregar;
        private readonly Exception _excepcionAlAgregar;
        public int IdCompraAAsignar { get; set; } = 1;
        public bool AddOrEditCompraFueLlamado { get; private set; }

        public FakeCompraRepository(Contratos.IUnitOfWork unitOfWorkAEntregar = null, Exception excepcionAlAgregar = null)
        {
            _unitOfWorkAEntregar = unitOfWorkAEntregar;
            _excepcionAlAgregar = excepcionAlAgregar;
        }

        public Contratos.IUnitOfWork IniciarUnitOfWork() => _unitOfWorkAEntregar;

        public int addOrEditCompra(Compra oCompraE, Contratos.IUnitOfWork unitOfWork = null)
        {
            AddOrEditCompraFueLlamado = true;
            if (_excepcionAlAgregar != null) throw _excepcionAlAgregar;
            return IdCompraAAsignar;
        }

        public void anularCompra(int idCompra) => throw new NotImplementedException();
        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal) => throw new NotImplementedException();
        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal) => throw new NotImplementedException();
        public DataTable findById(int idCompra) => throw new NotImplementedException();
        public int agregarCompra(Compra oCompraE) => throw new NotImplementedException();
        public void ModificarCompra(Compra oCompraE) => throw new NotImplementedException();
        public void actualizarObservacionesCompra(int idCompra, string observaciones, int actualizadoPor) => throw new NotImplementedException();
        public List<int> obtenerPesajesVinculadosPorDestino(int idPesajeDestino) => throw new NotImplementedException();
        public Dictionary<int, List<int>> obtenerPesajesVinculadosPorDestinos(IEnumerable<int> idsDestino) => throw new NotImplementedException();
        public void actualizarIdPesajeAjustado(int idCompra, int? idPesajeAjustado, int actualizadoPor) => throw new NotImplementedException();
        public float getTotalCompra(int idCompra, string tipoCompra) => throw new NotImplementedException();
        public void modificarPrecioMedia(int idCompra, float precioKg) => throw new NotImplementedException();
        public void agregarCortePorCompra(CortePorCompra oCorteE, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public void limpiarCortesPorCompra(int idCompra) => throw new NotImplementedException();
        public void agregarMediaRes(MediaRes oMediaResE, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public int obtenerIdUltimaCompra() => throw new NotImplementedException();
        public DataTable obtenerCortesPorCompra(int idCompra) => throw new NotImplementedException();
        public DataTable obtenerMediasPorCompra(int idCompra) => throw new NotImplementedException();
        public void modificarMediaPorCompra(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void modificarCortePorCompra(CortePorCompra oCorteE, int idCompra) => throw new NotImplementedException();
        public void quitarStockMedia(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void quitarStockTeoricoMedia(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void quitarStockCorte(CortePorCompra oCorteE, int idCompra) => throw new NotImplementedException();
        public DataTable porcentajeCortesPorCompra(int idCompra) => throw new NotImplementedException();
        public DataTable getPromMedias(int idCompra) => throw new NotImplementedException();
        public DataTable getPorcCortesEnMedias(int idCompra) => throw new NotImplementedException();
        public int getIdAjusteDelPesaje(int idPesaje) => throw new NotImplementedException();
        public Dictionary<int, int> getIdsAjustePorPesajes(IEnumerable<int> idsPesaje) => throw new NotImplementedException();
        public void actualizarEstadoPesaje(int idPesaje, Compra.estadoAjusteStock estadoAjStock) => throw new NotImplementedException();
    }
}
