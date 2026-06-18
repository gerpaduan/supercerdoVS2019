using System;

namespace Entidades
{
    public class CatalogoGlobalImportacionProducto
    {
        public int IdCatalogoGlobalImportacionProducto { get; set; }
        public int IdEmpresa { get; set; }
        public int IdProductoGlobal { get; set; }
        public int IdProductoEmpresa { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? IdUsuarioAlta { get; set; }
    }
}
