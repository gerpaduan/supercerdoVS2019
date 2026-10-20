using System.Collections.Generic;
using System.Linq;
using Entidades;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre los filtros puros de "Existencia por sucursales" (Negocio.ExistenciaFiltros, docs/DECISIONS.md 2026-10-19):
    // seleccion multiple de Tipos / Marcas / Proveedores (OR dentro de una dimension, AND entre dimensiones) y el filtro
    // "Con movimiento" (salidas = ventas + consumo en elaborados, o dias con venta).
    public class ExistenciaFiltrosTests
    {
        private static Entidades.Corte Producto(int id, string tipo, int idMarca = 0)
        {
            return new Entidades.Corte
            {
                IdCorte = id,
                Tipo = tipo,
                Marca = idMarca > 0 ? new Entidades.Persona { IdPersona = idMarca } : null
            };
        }

        private static ExistenciaStockPorSucursalPlanoVm Fila(int idCorte, int idSucursal = 1)
        {
            return new ExistenciaStockPorSucursalPlanoVm { IdCorte = idCorte, IdSucursal = idSucursal };
        }

        private static ActividadStockVm Actividad(int idCorte, int idSucursal, float ventas = 0f, float elaboracion = 0f, int dias = 0)
        {
            return new ActividadStockVm
            {
                IdCorte = idCorte,
                IdSucursal = idSucursal,
                VentasVentana = ventas,
                ElaboracionVentana = elaboracion,
                DiasConVentaVentana = dias
            };
        }

        [Fact]
        public void CortesPorTipos_UneVariosTiposSinDistinguirMayusculasNiEspacios()
        {
            var cortes = new[] { Producto(1, "Vacuno"), Producto(2, " cerdo "), Producto(3, "Pollo"), Producto(4, null) };

            var ids = ExistenciaFiltros.CortesPorTipos(cortes, new[] { "VACUNO", "Cerdo", "" });

            Assert.Equal(new[] { 1, 2 }, ids.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void CortesPorMarcas_UneVariasMarcasYNuncaIncluyeProductosSinMarca()
        {
            var cortes = new[] { Producto(1, "A", 10), Producto(2, "A", 20), Producto(3, "A", 30), Producto(4, "A", 0) };

            var ids = ExistenciaFiltros.CortesPorMarcas(cortes, new[] { 10, 30, 0 });

            Assert.Equal(new[] { 1, 3 }, ids.OrderBy(x => x).ToArray());
        }

        private static ExistenciaFiltros.CompraProveedorProducto Compra(int idProveedor, string nombre, int idCorte)
        {
            return new ExistenciaFiltros.CompraProveedorProducto { IdProveedor = idProveedor, Proveedor = nombre, IdCorte = idCorte };
        }

        [Fact]
        public void CortesPorProveedores_JuntaLosProductosComprados_AUnoOMasProveedores()
        {
            var compras = new[]
            {
                Compra(1, "A", 10), Compra(1, "A", 11),
                Compra(2, "B", 11), Compra(2, "B", 12),
                Compra(3, "C", 13)
            };

            var ids = ExistenciaFiltros.CortesPorProveedores(compras, new[] { 1, 2, 0 });

            Assert.Equal(new[] { 10, 11, 12 }, ids.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void CortesPorProveedores_ProveedorSinCompras_DaConjuntoVacio_NoTodos()
        {
            // Vacio != null: un proveedor sin compras deja la lista vacia en vez de mostrar todo (ver Aplicar).
            var ids = ExistenciaFiltros.CortesPorProveedores(new[] { Compra(1, "A", 10) }, new[] { 99 });

            Assert.NotNull(ids);
            Assert.Empty(ids);
        }

        [Fact]
        public void ProveedoresDistintos_SinRepetirOrdenadosPorNombre_YSinLosQueNoTienenNombre()
        {
            var compras = new[]
            {
                Compra(2, "Zeta", 1), Compra(2, "Zeta", 2), Compra(1, "alfa", 3),
                Compra(3, "  ", 4), Compra(0, "Fantasma", 5), null
            };

            var proveedores = ExistenciaFiltros.ProveedoresDistintos(compras);

            Assert.Equal(new[] { "alfa", "Zeta" }, proveedores.Select(p => p.Value).ToArray());
            Assert.Equal(new[] { 1, 2 }, proveedores.Select(p => p.Key).ToArray());
        }

        [Fact]
        public void Aplicar_SinConjuntosActivos_DevuelveElPlanoSinTocar()
        {
            var plano = new List<ExistenciaStockPorSucursalPlanoVm> { Fila(1), Fila(2) };

            var resultado = ExistenciaFiltros.Aplicar(plano, null, null, null);

            Assert.Same(plano, resultado);
        }

        [Fact]
        public void Aplicar_UnConjunto_DejaSoloSusProductos_EnTodasLasSucursales()
        {
            var plano = new List<ExistenciaStockPorSucursalPlanoVm> { Fila(1, 1), Fila(1, 2), Fila(2, 1), Fila(3, 2) };

            var resultado = ExistenciaFiltros.Aplicar(plano, new HashSet<int> { 1, 3 });

            Assert.Equal(new[] { 1, 1, 3 }, resultado.Select(x => x.IdCorte).OrderBy(x => x).ToArray());
        }

        [Fact]
        public void Aplicar_VariasDimensiones_IntersectaEntreEllas()
        {
            // Tipo {1,2,3} Y Marca {2,3,4} Y Proveedor {3,4,5} -> solo el 3 cumple las tres.
            var plano = Enumerable.Range(1, 5).Select(i => Fila(i)).ToList();

            var resultado = ExistenciaFiltros.Aplicar(plano, new HashSet<int> { 1, 2, 3 }, null, new HashSet<int> { 3, 4, 5 }, new HashSet<int> { 2, 3, 4 });

            Assert.Equal(new[] { 3 }, resultado.Select(x => x.IdCorte).ToArray());
        }

        [Fact]
        public void Aplicar_ConjuntoVacio_NoDejaPasarNada()
        {
            // Un filtro activo que no coincide con ningun producto (ej. proveedor sin compras) deja la lista vacia,
            // no "todos": vacio es distinto de null.
            var plano = new List<ExistenciaStockPorSucursalPlanoVm> { Fila(1), Fila(2) };

            var resultado = ExistenciaFiltros.Aplicar(plano, new HashSet<int>());

            Assert.Empty(resultado);
        }

        [Fact]
        public void CortesConMovimiento_Salidas_SumaVentasYConsumoEnElaborados()
        {
            // Producto 1: solo se gasta en elaborados (4 kg). Producto 2: solo ventas (6 kg). Producto 3: casi nada.
            var actividad = new[]
            {
                Actividad(1, 1, ventas: 0f, elaboracion: 4f),
                Actividad(2, 1, ventas: 6f),
                Actividad(3, 1, ventas: 0.5f, elaboracion: 0.5f)
            };

            var ids = ExistenciaFiltros.CortesConMovimiento(actividad, new List<int>(), 4, ExistenciaFiltros.MedidaSalidas);

            Assert.Equal(new[] { 1, 2 }, ids.OrderBy(x => x).ToArray());
        }

        [Fact]
        public void CortesConMovimiento_DiasConVenta_IgnoraElConsumoEnElaborados()
        {
            // El insumo que solo se gasta en elaborados NO cuenta como "dias con venta".
            var actividad = new[]
            {
                Actividad(1, 1, ventas: 0f, elaboracion: 50f, dias: 0),
                Actividad(2, 1, ventas: 1f, dias: 3),
                Actividad(3, 1, ventas: 90f, dias: 1)
            };

            var ids = ExistenciaFiltros.CortesConMovimiento(actividad, null, 2, ExistenciaFiltros.MedidaDiasVenta);

            Assert.Equal(new[] { 2 }, ids.ToArray());
        }

        [Fact]
        public void CortesConMovimiento_CumpleSiAlgunaSucursalElegidaAlcanzaElMinimo()
        {
            // Producto 1: 3 kg en la sucursal 1 y 3 kg en la 2 (no se suman: el criterio es "alguna sucursal alcanza").
            // Producto 2: 9 kg pero en la sucursal 3, que no esta entre las elegidas.
            var actividad = new[]
            {
                Actividad(1, 1, ventas: 3f),
                Actividad(1, 2, ventas: 3f),
                Actividad(2, 3, ventas: 9f)
            };

            Assert.Empty(ExistenciaFiltros.CortesConMovimiento(actividad, new List<int> { 1, 2 }, 5, ExistenciaFiltros.MedidaSalidas));
            Assert.Equal(new[] { 2 }, ExistenciaFiltros.CortesConMovimiento(actividad, new List<int> { 3 }, 5, ExistenciaFiltros.MedidaSalidas).ToArray());
            Assert.Equal(new[] { 1, 2 }, ExistenciaFiltros.CortesConMovimiento(actividad, new List<int>(), 3, ExistenciaFiltros.MedidaSalidas).OrderBy(x => x).ToArray());
        }

        [Fact]
        public void CortesConMovimiento_ProductoSinActividad_NoApareceNunca()
        {
            // El repositorio solo devuelve pares con alguna salida: un producto ausente del listado esta "sin movimiento".
            var ids = ExistenciaFiltros.CortesConMovimiento(new ActividadStockVm[0], null, 1, ExistenciaFiltros.MedidaSalidas);

            Assert.Empty(ids);
        }
    }
}
