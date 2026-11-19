// Catalogo de las cargas que el cajero abre desde el POS (2026-11-18, ver docs/DECISIONS.md "Cargas desde el POS"):
// ingreso/egreso/pesaje/ajuste de stock, movimiento, elaborado nuevo e ingreso rapido de elaborado. Una sola fuente para el
// endpoint VentasController.AbrirCargaPOS (que arma la URL y valida el permiso) y para los tests. El cliente manda solo la
// clave ("ingreso", "movimiento"...): nunca una URL ni un permiso.
namespace WebCore.Helpers
{
    public sealed class CargaPos
    {
        // Clave que viaja del navegador (slot del POS): una carga abierta de cada una a la vez.
        public string Tipo { get; init; } = "";
        public string Titulo { get; init; } = "";
        // Accion de autorizacion provisoria que habilita esta carga si el operador no tiene el permiso.
        public string Accion { get; init; } = "";
        // Permiso que exige la pantalla destino para dar de alta (el mismo que ya chequea su controller).
        public string Permiso { get; init; } = "";
        // Solo administradores (el ajuste de stock): el permiso comun no alcanza.
        public bool SoloAdmin { get; init; }
        public string Controller { get; init; } = "";
        public string Action { get; init; } = "";
        // Solo Stock: tipo de movimiento de stock con el que arranca el formulario.
        public string? TipoCompra { get; init; }
    }

    public static class CargasPosCatalogo
    {
        public static readonly IReadOnlyList<CargaPos> Todas = new List<CargaPos>
        {
            new CargaPos { Tipo = "ingreso", Titulo = "Ingreso de stock", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaStock,
                Permiso = Entidades.Permisos.Stock.AddOrEditStock, Controller = "Stock", Action = "Nuevo", TipoCompra = "Ingreso Stock" },
            new CargaPos { Tipo = "egreso", Titulo = "Egreso de stock", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaStock,
                Permiso = Entidades.Permisos.Stock.AddOrEditStock, Controller = "Stock", Action = "Nuevo", TipoCompra = "Egreso Stock" },
            new CargaPos { Tipo = "pesaje", Titulo = "Pesaje de cortes", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaStock,
                Permiso = Entidades.Permisos.Stock.AddOrEditStock, Controller = "Stock", Action = "Nuevo", TipoCompra = "Pesaje Cortes" },
            new CargaPos { Tipo = "ajuste", Titulo = "Ajuste de stock", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaStockAjuste,
                Permiso = Entidades.Permisos.Stock.AddOrEditStock, SoloAdmin = true, Controller = "Stock", Action = "Nuevo", TipoCompra = "Ajuste Stock" },
            new CargaPos { Tipo = "movimiento", Titulo = "Movimiento", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaMovimiento,
                Permiso = Entidades.Permisos.Movimiento.NuevoMovimiento, Controller = "Movimientos", Action = "Nuevo" },
            new CargaPos { Tipo = "elaborado", Titulo = "Nuevo elaborado", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaElaborado,
                Permiso = Entidades.Permisos.Elaborado.IngresoEmbutido, Controller = "Elaborados", Action = "Carga" },
            new CargaPos { Tipo = "rapido", Titulo = "Ingreso rápido de elaborado", Accion = Negocio.AutorizacionesProvisoriasPos.AccionCargaElaboradoRapido,
                Permiso = Entidades.Permisos.Elaborado.IngresoEmbutidoRapido, Controller = "Elaborados", Action = "IngresoRapido" }
        };

        // La carga de esa clave, o null si no existe (el cliente manda el texto: se valida contra el catalogo).
        public static CargaPos? Buscar(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return null;
            foreach (var carga in Todas)
                if (string.Equals(carga.Tipo, tipo.Trim(), StringComparison.Ordinal)) return carga;
            return null;
        }
    }
}
