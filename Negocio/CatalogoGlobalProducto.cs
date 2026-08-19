using System;
using System.Collections.Generic;
using Utilidades;

namespace Negocio
{
    /// <summary>
    /// Envoltorio fino sobre Datos.CatalogoGlobalProducto, mismo patron que Negocio.Corte.
    /// </summary>
    public class CatalogoGlobalProducto
    {
        private readonly Contratos.ICatalogoGlobalProductoRepository oCatalogoGlobalD;

        public CatalogoGlobalProducto(IEmpresaContext empresa, IParametrosContext param = null)
        {
            oCatalogoGlobalD = new Datos.CatalogoGlobalProducto(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // ICatalogoGlobalProductoRepository (ej. DatosPostgres.CatalogoGlobalProductoPg).
        public CatalogoGlobalProducto(Contratos.ICatalogoGlobalProductoRepository repositorio)
        {
            oCatalogoGlobalD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public Entidades.CatalogoGlobalProducto findCorteGlobalByCodigo(long codigo, bool buscarMaestro)
        {
            return oCatalogoGlobalD.findCorteGlobalByCodigo(codigo, buscarMaestro);
        }

        public List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPagina(string busqueda, string tipo, int pagina, int cantidad, int cantidadExtra)
        {
            return oCatalogoGlobalD.ObtenerCatalogoGlobalPagina(busqueda, tipo, pagina, cantidad, cantidadExtra);
        }

        public List<string> ObtenerTiposCatalogoGlobal()
        {
            return oCatalogoGlobalD.ObtenerTiposCatalogoGlobal();
        }

        public List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPorIds(IEnumerable<int> idsCortes)
        {
            return oCatalogoGlobalD.ObtenerCatalogoGlobalPorIds(idsCortes);
        }
    }
}
