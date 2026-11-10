// Tests de los permisos de `formularios` que hasta 2026-11-10 no controlaban nada (docs/DECISIONS.md, "Permisos que no
// controlaban nada"): Productos / Tipos de producto (modificar), Etiquetas y los seis reportes. Usan las claves reales de
// la tabla `formularios` y Negocio.Usuario (el mismo chequeo que usan los controllers).
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class PermisosSinControlTests
    {
        private sealed class Empresa : Utilidades.IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private static readonly Negocio.Usuario NegocioUsuario = new Negocio.Usuario(new Empresa());

        private static PermisosUsuarios Otorgado(int idForm, string consulta, string edicion, string extra1, int diasVer, int diasEditar, bool propios = true) =>
            new PermisosUsuarios
            {
                IdForm = idForm,
                DiasPermitidosVer = diasVer,
                DiasPermitidosEditar = diasEditar,
                SoloRegistrosPropios = propios,
                Formulario = new Formulario { IdForm = idForm, FormConsulta = consulta, FormEdicion = edicion, FormEdicionExtra1 = extra1, FormEdicionExtra2 = "" }
            };

        // Fila 13 "Productos" y 14 "Tipos Productos" tal cual estan en la base (claves de consulta y de edicion).
        private static PermisosUsuarios Productos(int ver, int editar, bool propios = true) =>
            Otorgado(IdFormPermiso.Productos, "formCortes", "formNuevoCorte", "formModificarPrecios", ver, editar, propios);

        private static PermisosUsuarios TiposProducto(int ver, int editar) =>
            Otorgado(IdFormPermiso.TiposProducto, "formTiposProducto", "formAddOrEditTipoProducto", "", ver, editar);

        // Filas 25-30: consulta = edicion = nombre del reporte.
        private static PermisosUsuarios Reporte(int idForm, string clave, int ver) =>
            Otorgado(idForm, clave, clave, "", ver, -1);

        private static Entidades.Usuario Usuario(params PermisosUsuarios[] filas) =>
            new Entidades.Usuario { Id = 40, Activo = true, Permisos = new List<PermisosUsuarios>(filas) };

        private static Entidades.Usuario Admin() =>
            new Entidades.Usuario { Id = 1, Admin = true, Permisos = new List<PermisosUsuarios>() };

        // ---------- Productos y Tipos de producto: "modificar" ----------

        [Fact]
        public void LasClavesDeEdicionDelCodigo_CoincidenConLasFilasDeFormularios()
        {
            Assert.Equal("formCortes", Permisos.Producto.VerCortes);
            Assert.Equal("formNuevoCorte", Permisos.Producto.NuevoCorte);
            Assert.Equal("formModificarPrecios", Permisos.Producto.ModificarPrecios);
            Assert.Equal("formTiposProducto", Permisos.Producto.VerTiposProducto);
            Assert.Equal("formAddOrEditTipoProducto", Permisos.Producto.AddOrEditTipoProducto);
        }

        [Fact]
        public void ElChequeoViejo_ConIdCreadorMenosUno_NuncaMatcheabaLaClaveDeEdicion()
        {
            // Documenta el bug: aun con "puede editar" la clave de edicion no coincidia con tienePermiso(..., -1).
            var usuario = Usuario(Productos(30, 0));

            Assert.False(NegocioUsuario.tienePermiso(usuario, Permisos.Producto.NuevoCorte, DateTime.Today, -1));
            Assert.True(NegocioUsuario.tienePermiso(usuario, Permisos.Producto.VerCortes, DateTime.Today, -1));
        }

        [Fact]
        public void ConVerYModificar_PuedeCrearEditarYCambiarPrecios()
        {
            var usuario = Usuario(Productos(30, 0));

            Assert.True(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.NuevoCorte));
            Assert.True(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.ModificarPrecios));
        }

        [Fact]
        public void SoloVer_NoPuedeModificar()
        {
            var usuario = Usuario(Productos(30, -1), TiposProducto(30, -1));

            Assert.False(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.NuevoCorte));
            Assert.False(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.ModificarPrecios));
            Assert.False(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.AddOrEditTipoProducto));
        }

        [Fact]
        public void SinAcceso_NoPuedeNada()
        {
            var usuario = Usuario(Productos(-1, -1), TiposProducto(-1, -1));

            Assert.False(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.NuevoCorte));
            Assert.False(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.AddOrEditTipoProducto));
            Assert.False(NegocioUsuario.tienePermisoEdicion(Usuario(), Permisos.Producto.NuevoCorte));
        }

        [Fact]
        public void ModificarProductos_NoDaModificarTiposDeProducto_NiAlReves()
        {
            Assert.False(NegocioUsuario.tienePermisoEdicion(Usuario(Productos(30, 0), TiposProducto(30, -1)), Permisos.Producto.AddOrEditTipoProducto));
            Assert.False(NegocioUsuario.tienePermisoEdicion(Usuario(Productos(30, -1), TiposProducto(30, 0)), Permisos.Producto.NuevoCorte));
            Assert.True(NegocioUsuario.tienePermisoEdicion(Usuario(Productos(30, -1), TiposProducto(30, 0)), Permisos.Producto.AddOrEditTipoProducto));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ElAlcancePropiosOTodos_NoBloqueaLaEdicion(bool propios)
        {
            // No hay "registro creador" en Productos: el alcance no aplica y no debe dejar afuera a nadie.
            var usuario = Usuario(Productos(0, 0, propios));

            Assert.True(NegocioUsuario.tienePermisoEdicion(usuario, Permisos.Producto.NuevoCorte));
        }

        [Fact]
        public void ElAdminPuedeSiempre_YSinUsuarioNoPuedeNadie()
        {
            Assert.True(NegocioUsuario.tienePermisoEdicion(Admin(), Permisos.Producto.NuevoCorte));
            Assert.True(NegocioUsuario.tienePermisoEdicion(Admin(), Permisos.Producto.AddOrEditTipoProducto));
            Assert.False(NegocioUsuario.tienePermisoEdicion(null!, Permisos.Producto.NuevoCorte));
        }

        [Fact]
        public void Pedidos_ModificarUsaLaClaveDeEdicionDeStock()
        {
            // Pedidos usa Stock.AddOrEditStock (fila "Stocks"): con ver y editar >= 0 puede modificar, solo con ver no.
            var puede = Usuario(Otorgado(IdFormPermiso.Stocks, "formStock", "formAddOrEditStock", "", 30, 0));
            var soloVer = Usuario(Otorgado(IdFormPermiso.Stocks, "formStock", "formAddOrEditStock", "", 30, -1));

            Assert.True(NegocioUsuario.tienePermisoEdicion(puede, Permisos.Stock.AddOrEditStock));
            Assert.False(NegocioUsuario.tienePermisoEdicion(soloVer, Permisos.Stock.AddOrEditStock));
        }

        // ---------- Etiquetas ----------

        [Fact]
        public void Etiquetas_SeChequeaConElVer_YLaClaveEsLaDeLaTabla()
        {
            Assert.Equal("formEtiquetas", Permisos.Etiqueta.Etiquetas);

            var con = Usuario(Otorgado(IdFormPermiso.Etiquetas, "formEtiquetas", "formEtiquetas", "", 0, -1));
            var sin = Usuario(Otorgado(IdFormPermiso.Etiquetas, "formEtiquetas", "formEtiquetas", "", -1, -1));

            Assert.True(NegocioUsuario.tienePermiso(con, Permisos.Etiqueta.Etiquetas, DateTime.Today, -1));
            Assert.False(NegocioUsuario.tienePermiso(sin, Permisos.Etiqueta.Etiquetas, DateTime.Today, -1));
        }

        // ---------- Reportes ----------

        [Theory]
        [InlineData(PermisosReportes.StockActual, "Stock Actual")]
        [InlineData(PermisosReportes.CierreStock, "Cierre Stock")]
        [InlineData(PermisosReportes.StockRetroactivo, "Stock Retroactivo")]
        [InlineData(PermisosReportes.Proyeccion, "Proyeccion Ventas vs Stock")]
        [InlineData(PermisosReportes.VentasPorProducto, "Ventas por Producto")]
        [InlineData(PermisosReportes.BalanceEconomico, "Balance Económico")] // el selector va sin tilde, la clave de la tabla lleva tilde
        public void CadaTipoDeReporte_MapeaALaClaveRealDeFormularios(string tipo, string claveEnLaTabla)
        {
            Assert.Equal(claveEnLaTabla, PermisosReportes.ClaveDe(tipo));
        }

        [Fact]
        public void UnTipoDeReporteDesconocido_NoTieneClave_Ni_SePermite()
        {
            Assert.Null(PermisosReportes.ClaveDe("Reporte inventado"));
            Assert.False(PermisosReportes.Permite(NegocioUsuario, Admin(), "Reporte inventado", DateTime.Today));
        }

        [Fact]
        public void ElPermisoDeStocks_YaNoHabilitaLosReportes()
        {
            var usuario = Usuario(Otorgado(IdFormPermiso.Stocks, "formStock", "formAddOrEditStock", "", 30, 0));

            foreach (var tipo in PermisosReportes.TiposEnOrden)
                Assert.False(PermisosReportes.Permite(NegocioUsuario, usuario, tipo, DateTime.Today), tipo);
        }

        [Fact]
        public void CadaReporte_SeHabilitaSoloConSuPropiaFila()
        {
            var usuario = Usuario(Reporte(IdFormPermiso.ReporteVentasPorProducto, "Ventas por Producto", 30));

            Assert.True(PermisosReportes.Permite(NegocioUsuario, usuario, PermisosReportes.VentasPorProducto, DateTime.Today));
            Assert.False(PermisosReportes.Permite(NegocioUsuario, usuario, PermisosReportes.StockActual, DateTime.Today));
            Assert.False(PermisosReportes.Permite(NegocioUsuario, usuario, PermisosReportes.BalanceEconomico, DateTime.Today));
            Assert.Equal(new[] { PermisosReportes.VentasPorProducto }, PermisosReportes.TiposPermitidos(NegocioUsuario, usuario, DateTime.Today));
        }

        [Fact]
        public void ElBalanceEconomico_SeHabilitaConSuFila_AunqueElSelectorNoLleveTilde()
        {
            var usuario = Usuario(Reporte(IdFormPermiso.ReporteBalanceEconomico, "Balance Económico", 0));

            Assert.True(PermisosReportes.Permite(NegocioUsuario, usuario, "Balance Economico", DateTime.Today));
        }

        [Fact]
        public void LosDiasVer_LimitanHastaCuantoAtrasSePuedePonerLaFechaDesde()
        {
            var usuario = Usuario(Reporte(IdFormPermiso.ReporteStockActual, "Stock Actual", 7));

            Assert.True(PermisosReportes.Permite(NegocioUsuario, usuario, PermisosReportes.StockActual, DateTime.Today.AddDays(-7)));
            Assert.False(PermisosReportes.Permite(NegocioUsuario, usuario, PermisosReportes.StockActual, DateTime.Today.AddDays(-8)));
        }

        [Fact]
        public void SinAcceso_NoSePermiteNingunReporte_YElAdminTieneLosSeis()
        {
            var sin = Usuario(Reporte(IdFormPermiso.ReporteStockActual, "Stock Actual", -1));
            Assert.Empty(PermisosReportes.TiposPermitidos(NegocioUsuario, sin, DateTime.Today));

            Assert.Equal(PermisosReportes.TiposEnOrden, PermisosReportes.TiposPermitidos(NegocioUsuario, Admin(), DateTime.Today));
        }

        // ---------- Catalogo del asistente ----------

        [Fact]
        public void Catalogo_ProductosYTiposProducto_OfrecenVerYModificar_SinAvisoDeSoloAdmin()
        {
            foreach (int idForm in new[] { IdFormPermiso.Productos, IdFormPermiso.TiposProducto })
            {
                var def = CatalogoPermisos.Para(new Formulario { IdForm = idForm });
                Assert.Equal(TipoPermiso.VerYModificar, def.Tipo);
                Assert.True(string.IsNullOrEmpty(def.SinEfectoHoy));
                Assert.False(string.IsNullOrWhiteSpace(def.TextoModificar));
                Assert.DoesNotContain("administradores", def.Aviso);
            }
        }

        [Fact]
        public void Catalogo_Etiquetas_EsUnPermisoDeUnNivelDeProductos()
        {
            var def = CatalogoPermisos.Para(new Formulario { IdForm = IdFormPermiso.Etiquetas });

            Assert.Equal(TipoPermiso.SoloVer, def.Tipo);
            Assert.Equal(CatalogoPermisos.ModuloProductos, def.Modulo);
            Assert.True(string.IsNullOrEmpty(def.SinEfectoHoy));
        }

        [Fact]
        public void Catalogo_LosSeisReportes_SonVisibles_SoloVer_ConDiasVer()
        {
            int[] reportes = { 25, 26, 27, 28, 29, 30 };
            Assert.Equal(reportes, new[]
            {
                IdFormPermiso.ReporteStockActual, IdFormPermiso.ReporteCierreStock, IdFormPermiso.ReporteStockRetroactivo,
                IdFormPermiso.ReporteProyeccionVentasVsStock, IdFormPermiso.ReporteVentasPorProducto, IdFormPermiso.ReporteBalanceEconomico
            });

            foreach (int idForm in reportes)
            {
                var def = CatalogoPermisos.Para(new Formulario { IdForm = idForm });
                Assert.Equal(CatalogoPermisos.ModuloReportes, def.Modulo);
                Assert.Equal(TipoPermiso.SoloVer, def.Tipo);
                Assert.True(def.UsaDiasVer);
                Assert.False(def.UsaDiasEditar);
                Assert.True(string.IsNullOrEmpty(def.SinEfectoHoy));
            }
        }
    }
}
