// Filtros de "Existencia por sucursales" que NO viajan al motor de base de datos: seleccion multiple de Tipos / Marcas /
// Proveedores y el filtro "Con movimiento" (salidas = ventas + consumo en elaborados desde una fecha).
// Son funciones PURAS (sin base de datos): reciben listas ya leidas y devuelven conjuntos de idCorte o el plano filtrado.
// Tests: Negocio.Tests/ExistenciaFiltrosTests.cs. Quien las llama es Negocio.Corte.ObtenerExistenciaPlano (multiseleccion)
// y WebCore.StockController (movimiento). Detalle del porque: docs/DECISIONS.md (2026-10-19, Existencia por sucursales).
//
// Regla general: DENTRO de una dimension se une (OR: cualquiera de las marcas elegidas); ENTRE dimensiones se intersecta
// (AND: tipo Y marca Y proveedor). Un conjunto null significa "esa dimension no filtra".
//
// Sintaxis a proposito conservadora (C# 7.3): Negocio tambien compila para net472.
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;

namespace Negocio
{
    public static class ExistenciaFiltros
    {
        // Medidas del filtro "Con movimiento".
        public const string MedidaSalidas = "salidas";   // ventas + consumo en elaborados (kg/un.)
        public const string MedidaDiasVenta = "dias";    // dias distintos con venta (solo ventas)

        // idCorte de los productos cuyo Tipo coincide (sin distinguir mayusculas ni espacios en los bordes) con alguno de los elegidos.
        public static HashSet<int> CortesPorTipos(IEnumerable<Entidades.Corte> cortes, IEnumerable<string> tipos)
        {
            var elegidos = new HashSet<string>(
                (tipos ?? Enumerable.Empty<string>())
                    .Select(x => (x ?? "").Trim())
                    .Where(x => x.Length > 0),
                StringComparer.OrdinalIgnoreCase);

            var resultado = new HashSet<int>();
            foreach (var corte in cortes ?? Enumerable.Empty<Entidades.Corte>())
            {
                if (corte == null) continue;
                string tipo = (corte.Tipo ?? "").Trim();
                if (elegidos.Contains(tipo)) resultado.Add(corte.IdCorte);
            }
            return resultado;
        }

        // idCorte de los productos cuya marca es alguna de las elegidas. Un producto sin marca nunca coincide.
        public static HashSet<int> CortesPorMarcas(IEnumerable<Entidades.Corte> cortes, IEnumerable<int> idsMarcas)
        {
            var elegidas = new HashSet<int>((idsMarcas ?? Enumerable.Empty<int>()).Where(x => x > 0));

            var resultado = new HashSet<int>();
            foreach (var corte in cortes ?? Enumerable.Empty<Entidades.Corte>())
            {
                if (corte == null || corte.Marca == null) continue;
                if (elegidas.Contains(corte.Marca.IdPersona)) resultado.Add(corte.IdCorte);
            }
            return resultado;
        }

        // Una compra real de un producto a un proveedor (fila de Contratos.ICorteRepository.obtenerProductosCompradosPorProveedor).
        public sealed class CompraProveedorProducto
        {
            public int IdProveedor { get; set; }
            public string Proveedor { get; set; }
            public int IdCorte { get; set; }
        }

        // idCorte de los productos que se le compraron a alguno de los proveedores elegidos (union: cualquiera de ellos).
        public static HashSet<int> CortesPorProveedores(IEnumerable<CompraProveedorProducto> compras, IEnumerable<int> idsProveedores)
        {
            var elegidos = new HashSet<int>((idsProveedores ?? Enumerable.Empty<int>()).Where(x => x > 0));

            var resultado = new HashSet<int>();
            foreach (var compra in compras ?? Enumerable.Empty<CompraProveedorProducto>())
            {
                if (compra != null && elegidos.Contains(compra.IdProveedor)) resultado.Add(compra.IdCorte);
            }
            return resultado;
        }

        // Proveedores distintos (id -> nombre) de esas compras, ordenados por nombre. Sin nombre no se listan (no se podrian elegir a ojo).
        public static List<KeyValuePair<int, string>> ProveedoresDistintos(IEnumerable<CompraProveedorProducto> compras)
        {
            var porId = new Dictionary<int, string>();
            foreach (var compra in compras ?? Enumerable.Empty<CompraProveedorProducto>())
            {
                if (compra == null || compra.IdProveedor <= 0 || string.IsNullOrWhiteSpace(compra.Proveedor)) continue;
                if (!porId.ContainsKey(compra.IdProveedor)) porId[compra.IdProveedor] = compra.Proveedor.Trim();
            }

            return porId
                .OrderBy(x => x.Value, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.Key)
                .ToList();
        }

        // Deja solo las filas del plano cuyo idCorte esta en TODOS los conjuntos no nulos (AND entre dimensiones).
        // Sin ningun conjunto no nulo devuelve el plano tal cual (misma instancia: no hay nada que filtrar).
        public static List<ExistenciaStockPorSucursalPlanoVm> Aplicar(List<ExistenciaStockPorSucursalPlanoVm> plano, params HashSet<int>[] permitidosPorDimension)
        {
            if (plano == null) return new List<ExistenciaStockPorSucursalPlanoVm>();

            var activos = (permitidosPorDimension ?? new HashSet<int>[0]).Where(x => x != null).ToList();
            if (activos.Count == 0) return plano;

            return plano.Where(fila => activos.All(conjunto => conjunto.Contains(fila.IdCorte))).ToList();
        }

        // idCorte con movimiento desde la fecha pedida, para las sucursales indicadas (vacio = todas).
        // Un producto cumple si ALGUNA de esas sucursales alcanza el minimo (mismo criterio que el filtro de Estado).
        //  - MedidaSalidas: VentasVentana + ElaboracionVentana >= minimo (kg/un.; incluye el consumo de insumos en elaborados).
        //  - MedidaDiasVenta: DiasConVentaVentana >= minimo (solo ventas).
        // La actividad ya viene acotada a la ventana pedida (ver IActividadStockRepository.ObtenerActividadStock).
        public static HashSet<int> CortesConMovimiento(IEnumerable<ActividadStockVm> actividad, ICollection<int> idsSucursales, double minimo, string medida)
        {
            bool porDias = string.Equals(medida, MedidaDiasVenta, StringComparison.OrdinalIgnoreCase);
            bool todasLasSucursales = idsSucursales == null || idsSucursales.Count == 0;

            var resultado = new HashSet<int>();
            foreach (var fila in actividad ?? Enumerable.Empty<ActividadStockVm>())
            {
                if (fila == null) continue;
                if (!todasLasSucursales && !idsSucursales.Contains(fila.IdSucursal)) continue;

                double valor = porDias
                    ? fila.DiasConVentaVentana
                    : (double)fila.VentasVentana + (double)fila.ElaboracionVentana;

                if (valor >= minimo) resultado.Add(fila.IdCorte);
            }
            return resultado;
        }
    }
}
