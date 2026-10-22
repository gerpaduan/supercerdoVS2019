using System;
using System.Collections.Generic;
using System.Data;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake minimo de ICompraRepository, mismo criterio que FakeVentaRepository: solo los
    // metodos que Negocio.Compra.AddOrEditCompra realmente llama tienen cuerpo real
    // (IniciarUnitOfWork, addOrEditCompra, agregarCortePorCompra -- este ultimo no persiste
    // nada, solo registra la llamada). El resto tira NotImplementedException.
    public sealed class FakeCompraRepository : Contratos.ICompraRepository
    {
        private readonly Contratos.IUnitOfWork _unitOfWorkAEntregar;
        private readonly Exception _excepcionAlAgregar;
        public int IdCompraAAsignar { get; set; } = 1;
        public bool AddOrEditCompraFueLlamado { get; private set; }

        // actualizarEstadoPesaje (estado del ajuste de un Pesaje): se registra cuantas veces se llamo y si
        // la unidad de trabajo YA estaba completada en ese momento (en Postgres debe ser despues del commit,
        // si no se pisa con el lock de la fila que la propia transaccion modifico).
        public int ActualizarEstadoPesajeLlamadas { get; private set; }
        public bool? UnitOfWorkCompletadaAlActualizarEstado { get; private set; }
        public Exception ExcepcionAlActualizarEstado { get; set; }

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

        // Eliminacion con motivo (2026-10-22): configurable y registra lo recibido.
        public bool EliminarLlamado { get; private set; }
        public AuditoriaEliminacion AuditoriaRecibida { get; private set; }
        public Contratos.IUnitOfWork UnitOfWorkRecibida { get; private set; }
        public int IdAuditoriaADevolver { get; set; } = 77;
        public int eliminarCompraConAuditoria(int idCompra, AuditoriaEliminacion auditoria, Contratos.IUnitOfWork unitOfWork = null)
        {
            EliminarLlamado = true;
            AuditoriaRecibida = auditoria;
            UnitOfWorkRecibida = unitOfWork;
            return IdAuditoriaADevolver;
        }

        // Datos que lee Negocio.Compra.eliminarCompra / obtenerVistaPreviaEliminacion.
        public DataTable FilaCompra { get; set; }
        public DataTable CortesPorCompra { get; set; }
        public DataTable MediasPorCompra { get; set; }
        public List<int> PesajesVinculados { get; set; } = new List<int>();
        public int IdAjusteDelPesaje { get; set; }
        public DataTable obtenerCompras(int idSucursal, string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal) => throw new NotImplementedException();
        public DataTable getLineasCompras(int idSucursal, string tipoCompra, string texto, string codigo, string corte, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal) => throw new NotImplementedException();
        // Sin filas: Negocio.Compra.findById_convertToCompra devuelve una compra vacia (lo justo para
        // que estadoAjusteStock calcule "No realizado" sin necesitar Persona/Sucursal fakes).
        public DataTable findById(int idCompra) => FilaCompra ?? new DataTable();
        public int agregarCompra(Compra oCompraE) => throw new NotImplementedException();
        public void ModificarCompra(Compra oCompraE) => throw new NotImplementedException();
        public void actualizarObservacionesCompra(int idCompra, string observaciones, int actualizadoPor) => throw new NotImplementedException();
        public List<int> obtenerPesajesVinculadosPorDestino(int idPesajeDestino) => PesajesVinculados;
        public Dictionary<int, List<int>> obtenerPesajesVinculadosPorDestinos(IEnumerable<int> idsDestino) => throw new NotImplementedException();
        public void actualizarIdPesajeAjustado(int idCompra, int? idPesajeAjustado, int actualizadoPor) => throw new NotImplementedException();
        public float getTotalCompra(int idCompra, string tipoCompra) => throw new NotImplementedException();
        public void modificarPrecioMedia(int idCompra, float precioKg) => throw new NotImplementedException();
        public void agregarCortePorCompra(CortePorCompra oCorteE, Contratos.IUnitOfWork unitOfWork = null) { }
        public void limpiarCortesPorCompra(int idCompra) => throw new NotImplementedException();
        public void agregarMediaRes(MediaRes oMediaResE, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public int obtenerIdUltimaCompra() => throw new NotImplementedException();
        public DataTable obtenerCortesPorCompra(int idCompra) => CortesPorCompra ?? new DataTable();
        public DataTable obtenerMediasPorCompra(int idCompra) => MediasPorCompra ?? new DataTable();
        public void modificarMediaPorCompra(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void modificarCortePorCompra(CortePorCompra oCorteE, int idCompra) => throw new NotImplementedException();
        public void quitarStockMedia(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void quitarStockTeoricoMedia(MediaRes oMediaResE, int idCompra) => throw new NotImplementedException();
        public void quitarStockCorte(CortePorCompra oCorteE, int idCompra) => throw new NotImplementedException();
        public DataTable porcentajeCortesPorCompra(int idCompra) => throw new NotImplementedException();
        public DataTable getPromMedias(int idCompra) => throw new NotImplementedException();
        public DataTable getPorcCortesEnMedias(int idCompra) => throw new NotImplementedException();
        public int getIdAjusteDelPesaje(int idPesaje) => IdAjusteDelPesaje;
        public Dictionary<int, int> getIdsAjustePorPesajes(IEnumerable<int> idsPesaje) => throw new NotImplementedException();
        public void actualizarEstadoPesaje(int idPesaje, Compra.estadoAjusteStock estadoAjStock)
        {
            ActualizarEstadoPesajeLlamadas++;
            UnitOfWorkCompletadaAlActualizarEstado = (_unitOfWorkAEntregar as FakeUnitOfWork)?.CompletarLlamado;
            if (ExcepcionAlActualizarEstado != null) throw ExcepcionAlActualizarEstado;
        }
    }
}
