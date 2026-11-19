// Tests de las cargas que el cajero abre desde el POS (2026-11-18, docs/DECISIONS.md "Cargas desde el POS"): catalogo de
// cargas, acciones de autorizacion provisoria nuevas y deteccion del iframe que evita limpiar la sesion del POS.
using System.Linq;
using Negocio;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class CargasPosTests
    {
        private static readonly string[] TiposEsperados =
            { "ingreso", "egreso", "pesaje", "ajuste", "movimiento", "elaborado", "rapido" };

        [Fact]
        public void El_catalogo_tiene_las_siete_cargas_con_claves_unicas()
        {
            var claves = CargasPosCatalogo.Todas.Select(c => c.Tipo).ToList();
            Assert.Equal(TiposEsperados, claves);
            Assert.Equal(claves.Count, claves.Distinct().Count());
        }

        [Theory]
        [InlineData("ingreso", "Stock", "Nuevo", "Ingreso Stock")]
        [InlineData("egreso", "Stock", "Nuevo", "Egreso Stock")]
        [InlineData("pesaje", "Stock", "Nuevo", "Pesaje Cortes")]
        [InlineData("ajuste", "Stock", "Nuevo", "Ajuste Stock")]
        [InlineData("movimiento", "Movimientos", "Nuevo", null)]
        [InlineData("elaborado", "Elaborados", "Carga", null)]
        [InlineData("rapido", "Elaborados", "IngresoRapido", null)]
        public void Cada_carga_apunta_a_su_pantalla(string tipo, string controller, string action, string? tipoCompra)
        {
            var carga = CargasPosCatalogo.Buscar(tipo);
            Assert.NotNull(carga);
            Assert.Equal(controller, carga!.Controller);
            Assert.Equal(action, carga.Action);
            Assert.Equal(tipoCompra, carga.TipoCompra);
        }

        [Fact]
        public void Los_tipos_de_stock_son_los_textos_reales_de_Compra()
        {
            // StockController valida el tipo contra estos mismos textos: si cambian, la carga se abriria en un redirect.
            Assert.Equal(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock), CargasPosCatalogo.Buscar("ingreso")!.TipoCompra);
            Assert.Equal(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.EgresoStock), CargasPosCatalogo.Buscar("egreso")!.TipoCompra);
            Assert.Equal(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.PesajeCortes), CargasPosCatalogo.Buscar("pesaje")!.TipoCompra);
            Assert.Equal(Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.AjusteStock), CargasPosCatalogo.Buscar("ajuste")!.TipoCompra);
        }

        [Fact]
        public void Solo_el_ajuste_es_exclusivo_de_administradores()
        {
            Assert.Equal(new[] { "ajuste" }, CargasPosCatalogo.Todas.Where(c => c.SoloAdmin).Select(c => c.Tipo).ToArray());
        }

        [Fact]
        public void Cada_accion_del_catalogo_es_una_accion_autorizable_valida()
        {
            foreach (var carga in CargasPosCatalogo.Todas)
                Assert.True(AutorizacionesProvisoriasPos.EsAccionValida(carga.Accion), "Accion no valida: " + carga.Accion);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("INGRESO")]
        [InlineData("cierre")]
        [InlineData("http://otro.sitio/Stock/Nuevo")]
        public void Buscar_rechaza_lo_que_no_es_una_clave_del_catalogo(string? tipo)
        {
            // La comparacion es exacta (otra mayuscula no vale) y una URL nunca es una clave.
            Assert.Null(CargasPosCatalogo.Buscar(tipo));
        }

        [Fact]
        public void Buscar_ignora_los_espacios_de_los_costados()
        {
            Assert.NotNull(CargasPosCatalogo.Buscar(" ingreso "));
        }

        [Fact]
        public void Las_autorizaciones_de_carga_se_registran_y_se_consultan_por_pestana_y_accion()
        {
            var ahora = new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
            string estado = AutorizacionesProvisoriasPos.Registrar("", "pestana1", AutorizacionesProvisoriasPos.AccionCargaMovimiento,
                0, 7, 99, ahora, TimeSpan.FromMinutes(30));

            Assert.NotNull(AutorizacionesProvisoriasPos.Vigente(estado, "pestana1", AutorizacionesProvisoriasPos.AccionCargaMovimiento, 0, ahora));
            // Otra pestana, otra carga o una autorizacion de otro tipo de accion no valen.
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, "pestana2", AutorizacionesProvisoriasPos.AccionCargaMovimiento, 0, ahora));
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, "pestana1", AutorizacionesProvisoriasPos.AccionCargaStock, 0, ahora));
            // El ajuste no se habilita con la autorizacion de ingreso/egreso/pesaje.
            string estadoStock = AutorizacionesProvisoriasPos.Registrar("", "pestana1", AutorizacionesProvisoriasPos.AccionCargaStock,
                0, 7, 100, ahora, TimeSpan.FromMinutes(30));
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estadoStock, "pestana1", AutorizacionesProvisoriasPos.AccionCargaStockAjuste, 0, ahora));
        }

        [Fact]
        public void Una_autorizacion_de_carga_vence_y_se_puede_revocar()
        {
            var ahora = new DateTime(2026, 11, 18, 10, 0, 0, DateTimeKind.Utc);
            string estado = AutorizacionesProvisoriasPos.Registrar("", "pestana1", AutorizacionesProvisoriasPos.AccionCargaElaborado,
                0, 7, 55, ahora, TimeSpan.FromMinutes(30));

            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, "pestana1", AutorizacionesProvisoriasPos.AccionCargaElaborado, 0, ahora.AddMinutes(31)));

            string tras = AutorizacionesProvisoriasPos.Revocar(estado, "pestana1", AutorizacionesProvisoriasPos.AccionCargaElaborado, null, ahora, out var ids);
            Assert.Equal(new[] { 55 }, ids.ToArray());
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(tras, "pestana1", AutorizacionesProvisoriasPos.AccionCargaElaborado, 0, ahora));
        }

        [Theory]
        [InlineData("iframe", true)]
        [InlineData("IFRAME", true)]
        [InlineData(" iframe ", true)]
        [InlineData("document", false)]
        [InlineData("empty", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Solo_el_pedido_de_un_iframe_evita_limpiar_la_sesion(string? secFetchDest, bool esperado)
        {
            Assert.Equal(esperado, PermisosHelper.EsPedidoDeIframe(secFetchDest!));
        }
    }
}
