using System.Collections.Generic;

namespace Contratos
{
    // Espeja Datos.CatalogoGlobalProducto completo (4/4 metodos publicos). Catalogo global
    // (compartido entre todas las empresas, sin idEmpresa, sin RLS).
    public interface ICatalogoGlobalProductoRepository
    {
        Entidades.CatalogoGlobalProducto findCorteGlobalByCodigo(long codigo, bool buscarMaestro);
        List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPagina(string busqueda, string tipo, int pagina, int cantidad, int cantidadExtra);
        List<string> ObtenerTiposCatalogoGlobal();
        List<Entidades.CatalogoGlobalProducto> ObtenerCatalogoGlobalPorIds(IEnumerable<int> idsCortes);
    }
}
