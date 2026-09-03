using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace NegocioTests.Fakes
{
    // Implementacion en memoria de IFormatoCodigoBarrasRepository, solo para tests de
    // Negocio.BarcodeInterpreter y Negocio.FormatoCodigoBarras -- ningun metodo toca SQL
    // Server ni Postgres.
    public sealed class FakeFormatoCodigoBarrasRepository : Contratos.IFormatoCodigoBarrasRepository
    {
        private readonly List<FormatoCodigoBarras> _formatos = new List<FormatoCodigoBarras>();
        private int _nextId = 1;

        public FakeFormatoCodigoBarrasRepository AgregarFormato(int idEmpresa, int prefijo, int posicionCodigo, int longitudCodigo, int posicionValor, int longitudValor, TipoValorCodigoBarras tipoValor, int cantidadDecimales, bool activo = true, string nombre = null)
        {
            _formatos.Add(new FormatoCodigoBarras
            {
                Id = _nextId++,
                IdEmpresa = idEmpresa,
                Nombre = nombre ?? $"Formato prefijo {prefijo}",
                Prefijo = prefijo,
                LongitudTotal = 13,
                PosicionCodigo = posicionCodigo,
                LongitudCodigo = longitudCodigo,
                PosicionValor = posicionValor,
                LongitudValor = longitudValor,
                TipoValor = tipoValor,
                CantidadDecimales = cantidadDecimales,
                Activo = activo
            });
            return this;
        }

        public List<FormatoCodigoBarras> Listar(int idEmpresa)
        {
            return _formatos.Where(f => f.IdEmpresa == idEmpresa).OrderBy(f => f.Prioridad).ThenBy(f => f.Prefijo).ToList();
        }

        public FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa)
        {
            return _formatos.FirstOrDefault(f => f.Id == id && f.IdEmpresa == idEmpresa);
        }

        public FormatoCodigoBarras ObtenerActivoPorPrefijo(int idEmpresa, int prefijo)
        {
            return _formatos.FirstOrDefault(f => f.IdEmpresa == idEmpresa && f.Prefijo == prefijo && f.Activo);
        }

        public bool ExistePrefijo(int idEmpresa, int prefijo, int idExcluir)
        {
            return _formatos.Any(f => f.IdEmpresa == idEmpresa && f.Prefijo == prefijo && f.Id != idExcluir);
        }

        public void Agregar(FormatoCodigoBarras formato)
        {
            formato.Id = _nextId++;
            _formatos.Add(formato);
        }

        public void Actualizar(FormatoCodigoBarras formato)
        {
            int index = _formatos.FindIndex(f => f.Id == formato.Id);
            if (index >= 0) _formatos[index] = formato;
        }
    }
}
