using System.Collections.Generic;
using System.Linq;
using WebCore.Models;
using WebCore.Services;
using Xunit;

namespace WebCore.Tests
{
    // Filtros de Expendios generados: persona (por id o por identificacion) y filtros en vivo, con Producto que
    // deja solo las lineas que coinciden.
    public class ExpendiosGeneradosFiltroTests
    {
        private static ExpendioGeneradoVm Expendio(int id, string identificacion, int? idPersona = null, string nroRemito = "", string estado = "Pendiente", params (int codigo, string producto, decimal kg, decimal precio)[] lineas)
        {
            var vm = new ExpendioGeneradoVm
            {
                IdExpendio = id,
                IdentificacionExpendio = identificacion,
                IdPersona = idPersona,
                NroRemito = nroRemito,
                Estado = estado,
                Sucursal = "San Martín",
                Sector = "REMITOS",
                Usuario = "Germán"
            };
            foreach (var l in lineas)
                vm.Lineas.Add(new ExpendioGeneradoLineaVm { Codigo = l.codigo, Producto = l.producto, CantKg = l.kg, PrecioKg = l.precio, Total = l.kg * l.precio });
            vm.TotalKg = vm.Lineas.Sum(l => l.CantKg);
            vm.TotalImporte = vm.Lineas.Sum(l => l.Total);
            return vm;
        }

        private static readonly ISet<int> SinIds = new HashSet<int>();
        private static readonly ISet<string> SinIdentificaciones = new HashSet<string>();

        [Fact]
        public void Persona_PorIdGuardado()
        {
            Assert.True(ExpendiosGeneradosFiltro.CoincidePersona(7, "otro texto", new HashSet<int> { 7 }, SinIdentificaciones));
            Assert.False(ExpendiosGeneradosFiltro.CoincidePersona(8, "otro texto", new HashSet<int> { 7 }, SinIdentificaciones));
        }

        [Fact]
        public void Persona_PorIdentificacion_SinAcentosMayusculasNiEspacios()
        {
            var identificaciones = new HashSet<string> { ExpendiosGeneradosFiltro.NormalizarIdentificacion("  Cocina Reconquista ") };
            Assert.True(ExpendiosGeneradosFiltro.CoincidePersona(null, "COCINA RECONQUISTÁ", SinIds, identificaciones));
            Assert.True(ExpendiosGeneradosFiltro.CoincidePersona(null, "cocina reconquista  ", SinIds, identificaciones));
            Assert.False(ExpendiosGeneradosFiltro.CoincidePersona(null, "cocina reconquista 2", SinIds, identificaciones));
        }

        [Fact]
        public void Persona_IdentificacionVacia_NuncaCoincide()
        {
            var identificaciones = new HashSet<string> { "" };
            Assert.False(ExpendiosGeneradosFiltro.CoincidePersona(null, "", SinIds, identificaciones));
            Assert.False(ExpendiosGeneradosFiltro.CoincidePersona(null, "   ", SinIds, identificaciones));
        }

        [Fact]
        public void TextoFiltros_SinFiltros_EsVacio()
        {
            Assert.Equal("", ExpendiosGeneradosFiltro.TextoFiltros(new ExpendiosGeneradosFiltros { Sucursal = "San Martín" }, new string[0]));
        }

        [Fact]
        public void TextoFiltros_ConPersonasElegidas_ListaNombresYNoElTexto()
        {
            var filtros = new ExpendiosGeneradosFiltros { Estado = "Pendiente", Texto = "ignorado", Producto = "cerdo" };
            Assert.Equal(
                "Filtros: Estado: Pendiente  ·  Cliente: COCINA A, COCINA B  ·  Producto: cerdo",
                ExpendiosGeneradosFiltro.TextoFiltros(filtros, new[] { "COCINA A", "COCINA B" }));
        }

        [Fact]
        public void TextoFiltros_SinPersonas_UsaElTextoEscrito()
        {
            Assert.Equal("Filtros: Cliente / remito: reconquista",
                ExpendiosGeneradosFiltro.TextoFiltros(new ExpendiosGeneradosFiltros { Texto = " reconquista " }, new string[0]));
        }

        [Fact]
        public void Producto_DejaSoloLasLineasQueCoinciden_YRecalculaTotales()
        {
            var item = Expendio(1, "x", lineas: new[] { (228, "Pulpa Cerdo Molida", 10m, 100m), (300, "Bondiola", 5m, 200m) });
            var r = ExpendiosGeneradosFiltro.Aplicar(new[] { item }, new ExpendiosGeneradosFiltros { Producto = "cerdo" });

            Assert.Single(r);
            Assert.Single(r[0].Lineas);
            Assert.Equal(10m, r[0].TotalKg);
            Assert.Equal(1000m, r[0].TotalImporte);
            // El original no se toca.
            Assert.Equal(2, item.Lineas.Count);
        }

        [Fact]
        public void Producto_PorCodigo()
        {
            var item = Expendio(1, "x", lineas: new[] { (228, "Pulpa", 1m, 1m), (300, "Bondiola", 1m, 1m) });
            var r = ExpendiosGeneradosFiltro.Aplicar(new[] { item }, new ExpendiosGeneradosFiltros { Producto = "300" });
            Assert.Equal("Bondiola", r[0].Lineas[0].Producto);
        }

        [Fact]
        public void Producto_SinCoincidencias_QuitaElExpendio()
        {
            var item = Expendio(1, "x", lineas: new[] { (228, "Pulpa", 1m, 1m) });
            Assert.Empty(ExpendiosGeneradosFiltro.Aplicar(new[] { item }, new ExpendiosGeneradosFiltros { Producto = "bondiola" }));
        }

        [Fact]
        public void Texto_BuscaEnIdentificacionYNroRemito()
        {
            var items = new[]
            {
                Expendio(1, "Cocina Reconquista"),
                Expendio(2, "Otro", nroRemito: "0001-00000099")
            };
            Assert.Equal(new[] { 1 }, ExpendiosGeneradosFiltro.Aplicar(items, new ExpendiosGeneradosFiltros { Texto = "reconquista" }).Select(x => x.IdExpendio));
            Assert.Equal(new[] { 2 }, ExpendiosGeneradosFiltro.Aplicar(items, new ExpendiosGeneradosFiltros { Texto = "00000099" }).Select(x => x.IdExpendio));
        }

        [Fact]
        public void EstadoSucursalSectorUsuario_SonCoincidenciaExacta()
        {
            var items = new[] { Expendio(1, "a", estado: "Pendiente"), Expendio(2, "b", estado: "Asignado") };
            Assert.Equal(new[] { 2 }, ExpendiosGeneradosFiltro.Aplicar(items, new ExpendiosGeneradosFiltros { Estado = "asignado" }).Select(x => x.IdExpendio));
            Assert.Empty(ExpendiosGeneradosFiltro.Aplicar(items, new ExpendiosGeneradosFiltros { Sucursal = "San" }));
            Assert.Equal(2, ExpendiosGeneradosFiltro.Aplicar(items, new ExpendiosGeneradosFiltros { Sector = "remitos", Usuario = "germán" }).Count);
        }
    }
}
