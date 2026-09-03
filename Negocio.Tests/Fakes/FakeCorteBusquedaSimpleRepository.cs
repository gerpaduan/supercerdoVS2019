using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace NegocioTests.Fakes
{
    // Implementacion en memoria de ICorteBusquedaSimpleRepository, solo para tests de
    // Negocio.BarcodeInterpreter -- ningun metodo toca SQL Server ni Postgres. Simula el
    // catalogo de Corte de una o mas empresas, indexado por (IdEmpresa, Codigo).
    public sealed class FakeCorteBusquedaSimpleRepository : Contratos.ICorteBusquedaSimpleRepository
    {
        private readonly List<Corte> _cortes = new List<Corte>();

        public FakeCorteBusquedaSimpleRepository AgregarCorte(int idEmpresa, long codigo, string descripcion = "Producto de prueba", float precioKg = 100f, bool pesable = true)
        {
            _cortes.Add(new Corte
            {
                IdCorte = _cortes.Count + 1,
                IdEmpresa = idEmpresa,
                Codigo = codigo,
                CorteDesc = descripcion,
                PrecioKg = precioKg,
                Pesable = pesable
            });
            return this;
        }

        public Corte findCorteByCodigo(long codigo, bool buscarMaestro)
        {
            return _cortes.FirstOrDefault(c => c.Codigo == codigo);
        }

        public Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro)
        {
            return _cortes.FirstOrDefault(c => c.Codigo == codigo && c.IdEmpresa == idEmpresa);
        }
    }
}
