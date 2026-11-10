using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md): reglas puras de los
    // comprobantes (ValidacionComprobanteCompra), el servicio Negocio.ComprobanteCompra y el permiso del control fiscal.
    // Sin base de datos: repositorios en memoria.
    public class ComprobanteCompraTests
    {
        private const int IdEmpresa = 1;
        private const int IdCompra = 100;
        private const int IdProveedor = 7;
        private const string Cortes = "Cortes";

        private static ComprobanteCompra Factura(decimal total = 1210m, long numero = 1902, string tipo = TiposComprobanteCompra.FacturaA)
        {
            // Neto + 21 % de IVA.
            decimal neto, iva;
            Negocio.ValidacionComprobanteCompra.CalcularDesdeTotal(total, 21m, out neto, out iva);
            return new ComprobanteCompra
            {
                IdCompra = IdCompra, TipoComprobante = tipo, PtoVta = 3, Numero = numero,
                Fecha = new DateTime(2026, 11, 7), NetoGravado = neto, Iva = iva, Total = total, AlicuotaIva = 21m
            };
        }

        private static (Negocio.ComprobanteCompra negocio, FakeComprobanteCompraRepository repo) CrearSut(string tipoCompra = Cortes, decimal totalCompra = 1500m)
        {
            var repo = new FakeComprobanteCompraRepository();
            repo.AgregarCompra(IdCompra, IdProveedor, tipoCompra, totalCompra);
            var titulares = new FakeTitularFiscalRepository();
            titulares.AgregarPrincipal(IdEmpresa, 20306210786);
            return (new Negocio.ComprobanteCompra(repo, new Negocio.TitularFiscal(titulares), IdEmpresa), repo);
        }

        // ------------------------------------------------------------------ reglas puras

        [Fact]
        public void Validar_FacturaCorrecta_DaNull()
        {
            Assert.Null(Negocio.ValidacionComprobanteCompra.Validar(Factura(), new List<ComprobanteCompra>(), Cortes));
        }

        [Fact]
        public void Validar_CompraDeStock_Rechaza()
        {
            string error = Negocio.ValidacionComprobanteCompra.Validar(Factura(), new List<ComprobanteCompra>(), "Ingreso Stock");
            Assert.Contains("stock", error);
        }

        [Fact]
        public void Validar_TotalQueNoCoincideConNetoMasIva_Rechaza()
        {
            var f = Factura();
            f.Total += 5m;
            Assert.Contains("no coincide", Negocio.ValidacionComprobanteCompra.Validar(f, new List<ComprobanteCompra>(), Cortes));
        }

        [Fact]
        public void Validar_DiferenciaDeUnCentavo_SeAcepta()
        {
            var f = Factura();
            f.Total += Negocio.ValidacionComprobanteCompra.ToleranciaImporte;
            Assert.Null(Negocio.ValidacionComprobanteCompra.Validar(f, new List<ComprobanteCompra>(), Cortes));
        }

        [Fact]
        public void Validar_SegundaFacturaActiva_Rechaza()
        {
            var primera = Factura(numero: 1);
            primera.IdComprobante = 1;
            string error = Negocio.ValidacionComprobanteCompra.Validar(Factura(numero: 2), new List<ComprobanteCompra> { primera }, Cortes);
            Assert.Contains("ya tiene una factura", error);
        }

        [Fact]
        public void Validar_NotaDeCreditoSinFactura_Rechaza()
        {
            var nota = Factura(200m, tipo: TiposComprobanteCompra.NotaCreditoA);
            Assert.Contains("primero su factura", Negocio.ValidacionComprobanteCompra.Validar(nota, new List<ComprobanteCompra>(), Cortes));
        }

        [Fact]
        public void Validar_NotaDeCreditoConFactura_DaNull()
        {
            var factura = Factura();
            factura.IdComprobante = 1;
            var nota = Factura(200m, numero: 55, tipo: TiposComprobanteCompra.NotaCreditoA);
            Assert.Null(Negocio.ValidacionComprobanteCompra.Validar(nota, new List<ComprobanteCompra> { factura }, Cortes));
        }

        [Fact]
        public void FacturadoNeto_FacturaMasDebitoMenosCredito()
        {
            var lista = new List<ComprobanteCompra>
            {
                Factura(1000m),
                Factura(100m, numero: 2, tipo: TiposComprobanteCompra.NotaDebitoA),
                Factura(300m, numero: 3, tipo: TiposComprobanteCompra.NotaCreditoA),
            };
            Assert.Equal(800m, Negocio.ValidacionComprobanteCompra.FacturadoNeto(lista));
            Assert.Equal(700m, Negocio.ValidacionComprobanteCompra.SinFactura(1500m, 800m));
        }

        [Theory]
        [InlineData(1210, 21, 1000.00, 210.00)]
        [InlineData(1105, 10.5, 1000.00, 105.00)]
        [InlineData(100, 21, 82.64, 17.36)]
        public void CalcularDesdeTotal_NetoMasIvaDaElTotalExacto(double total, double alicuota, double netoEsperado, double ivaEsperado)
        {
            decimal neto, iva;
            Negocio.ValidacionComprobanteCompra.CalcularDesdeTotal((decimal)total, (decimal)alicuota, out neto, out iva);
            Assert.Equal((decimal)netoEsperado, neto);
            Assert.Equal((decimal)ivaEsperado, iva);
            Assert.Equal((decimal)total, neto + iva);
        }

        [Fact]
        public void EstadoResultante_SinFacturaVuelveAPendiente_ConFacturaEsFacturada_NoLlevaSeRespeta()
        {
            var sinNada = new List<ComprobanteCompra>();
            Assert.Equal(EstadoFacturaCompra.Pendiente, Negocio.ValidacionComprobanteCompra.EstadoResultante(sinNada, EstadoFacturaCompra.Facturada));
            Assert.Equal(EstadoFacturaCompra.NoLleva, Negocio.ValidacionComprobanteCompra.EstadoResultante(sinNada, EstadoFacturaCompra.NoLleva));
            Assert.Equal(EstadoFacturaCompra.Facturada,
                Negocio.ValidacionComprobanteCompra.EstadoResultante(new List<ComprobanteCompra> { Factura() }, EstadoFacturaCompra.NoLleva));
        }

        // ------------------------------------------------------------------ servicio

        [Fact]
        public void Guardar_Factura_MarcaLaCompraComoFacturada_YElDetalleMuestraLaDiferencia()
        {
            var (negocio, repo) = CrearSut(totalCompra: 1500m);
            var factura = Factura(1210m);

            int id;
            string error = negocio.Guardar(factura, 5, out id);

            Assert.Null(error);
            Assert.True(id > 0);
            Assert.Equal(EstadoFacturaCompra.Facturada, repo.Compras[IdCompra].EstadoFactura);

            var detalle = negocio.ObtenerDetalle(IdCompra);
            Assert.Equal(1500m, detalle.TotalCompra);
            Assert.Equal(1210m, detalle.FacturadoNeto);
            Assert.Equal(290m, detalle.SinFactura);
        }

        [Fact]
        public void Guardar_FacturaIgualAlTotalDeLaCompra_NoDejaDiferencia()
        {
            var (negocio, _) = CrearSut(totalCompra: 1210m);
            int id;
            Assert.Null(negocio.Guardar(Factura(1210m), 5, out id));
            Assert.Equal(0m, negocio.ObtenerDetalle(IdCompra).SinFactura);
        }

        [Fact]
        public void Guardar_ComprobanteDuplicado_Rechaza()
        {
            var (negocio, repo) = CrearSut();
            repo.AgregarCompra(101, IdProveedor, Cortes, 500m);
            int id;
            Assert.Null(negocio.Guardar(Factura(numero: 1902), 5, out id));

            var repetida = Factura(numero: 1902);
            repetida.IdCompra = 101;
            string error = negocio.Guardar(repetida, 5, out id);

            Assert.Contains("ya está cargado", error);
        }

        [Fact]
        public void Guardar_NotaDeCredito_RestaDelFacturado_SinTocarElEstado()
        {
            var (negocio, repo) = CrearSut(totalCompra: 1500m);
            int id;
            Assert.Null(negocio.Guardar(Factura(1210m), 5, out id));
            Assert.Null(negocio.Guardar(Factura(121m, numero: 9, tipo: TiposComprobanteCompra.NotaCreditoA), 5, out id));

            var detalle = negocio.ObtenerDetalle(IdCompra);
            Assert.Equal(1089m, detalle.FacturadoNeto);
            Assert.Equal(EstadoFacturaCompra.Facturada, repo.Compras[IdCompra].EstadoFactura);
        }

        [Fact]
        public void Eliminar_LaFactura_VuelveAPendiente_PeroNoSiQuedanNotas()
        {
            var (negocio, repo) = CrearSut();
            int idFactura, idNota;
            Assert.Null(negocio.Guardar(Factura(1210m), 5, out idFactura));
            Assert.Null(negocio.Guardar(Factura(121m, numero: 9, tipo: TiposComprobanteCompra.NotaCreditoA), 5, out idNota));

            Assert.Contains("Primero eliminá las notas", negocio.Eliminar(idFactura, 5, "error de carga"));

            Assert.Null(negocio.Eliminar(idNota, 5, "error de carga"));
            Assert.Null(negocio.Eliminar(idFactura, 5, "error de carga"));
            Assert.Equal(EstadoFacturaCompra.Pendiente, repo.Compras[IdCompra].EstadoFactura);
        }

        [Fact]
        public void Eliminar_SinMotivo_Rechaza()
        {
            var (negocio, _) = CrearSut();
            int id;
            negocio.Guardar(Factura(), 5, out id);
            Assert.Contains("motivo", negocio.Eliminar(id, 5, "  "));
        }

        [Fact]
        public void MarcarSinFactura_SoloSiNoHayComprobantes()
        {
            var (negocio, repo) = CrearSut();
            Assert.Null(negocio.MarcarSinFactura(IdCompra, 5));
            Assert.Equal(EstadoFacturaCompra.NoLleva, repo.Compras[IdCompra].EstadoFactura);

            // Con una factura cargada, la compra pasa a FACTURADA y ya no se puede marcar.
            int id;
            Assert.Null(negocio.Guardar(Factura(), 5, out id));
            Assert.Contains("comprobantes", negocio.MarcarSinFactura(IdCompra, 5));
        }

        [Fact]
        public void MarcarSinFactura_EnCompraDeStock_Rechaza()
        {
            var (negocio, _) = CrearSut(tipoCompra: "Ingreso Stock");
            Assert.Contains("stock", negocio.MarcarSinFactura(IdCompra, 5));
        }

        [Fact]
        public void CambiarTitular_ConComprobantes_PideConfirmacion_ConfirmadoActualizaCompraYMovimientos()
        {
            var repo = new FakeComprobanteCompraRepository();
            repo.AgregarCompra(IdCompra, IdProveedor, Cortes, 1500m);
            var titulares = new FakeTitularFiscalRepository();
            titulares.AgregarPrincipal(IdEmpresa, 20306210786);
            var segundo = titulares.AgregarNoPrincipal(IdEmpresa, 30500010912, 2, "Titular 2");
            var negocio = new Negocio.ComprobanteCompra(repo, new Negocio.TitularFiscal(titulares), IdEmpresa);

            int id;
            Assert.Null(negocio.Guardar(Factura(), 5, out id));

            bool requiere;
            string aviso = negocio.CambiarTitular(IdCompra, segundo.IdTitular, 5, false, out requiere);
            Assert.True(requiere);
            Assert.NotNull(aviso);
            Assert.Null(repo.Compras[IdCompra].IdTitular);

            Assert.Null(negocio.CambiarTitular(IdCompra, segundo.IdTitular, 5, true, out requiere));
            Assert.Equal(segundo.IdTitular, repo.Compras[IdCompra].IdTitular);
            Assert.Equal(segundo.IdTitular, repo.TitularMovimientos[IdCompra]);
        }

        [Fact]
        public void CambiarTitular_TitularDeshabilitado_Rechaza()
        {
            var repo = new FakeComprobanteCompraRepository();
            repo.AgregarCompra(IdCompra, IdProveedor, Cortes, 1500m);
            var titulares = new FakeTitularFiscalRepository();
            titulares.AgregarPrincipal(IdEmpresa, 20306210786);
            var segundo = titulares.AgregarNoPrincipal(IdEmpresa, 30500010912, 2, "Titular 2");
            segundo.Habilitado = false;
            var negocio = new Negocio.ComprobanteCompra(repo, new Negocio.TitularFiscal(titulares), IdEmpresa);

            bool requiere;
            Assert.Contains("no está habilitado", negocio.CambiarTitular(IdCompra, segundo.IdTitular, 5, true, out requiere));
        }

        [Fact]
        public void CambiarTitular_TitularDeOtraEmpresa_Rechaza()
        {
            var repo = new FakeComprobanteCompraRepository();
            repo.AgregarCompra(IdCompra, IdProveedor, Cortes, 1500m);
            var titulares = new FakeTitularFiscalRepository();
            titulares.AgregarPrincipal(IdEmpresa, 20306210786);
            var ajeno = titulares.AgregarNoPrincipal(99, 27230569040, 3);
            var negocio = new Negocio.ComprobanteCompra(repo, new Negocio.TitularFiscal(titulares), IdEmpresa);

            bool requiere;
            Assert.Contains("no existe", negocio.CambiarTitular(IdCompra, ajeno.IdTitular, 5, true, out requiere));
        }

        // ------------------------------------------------------------------ varios titulares

        [Fact]
        public void TieneVariosTitulares_SoloConDosOMas()
        {
            var titulares = new FakeTitularFiscalRepository();
            titulares.AgregarPrincipal(IdEmpresa, 20306210786);
            var negocio = new Negocio.TitularFiscal(titulares);
            Assert.False(negocio.TieneVariosTitulares(IdEmpresa));

            titulares.AgregarNoPrincipal(IdEmpresa, 30500010912, 2);
            Assert.True(negocio.TieneVariosTitulares(IdEmpresa));
        }

        // ------------------------------------------------------------------ permiso del control fiscal

        private static Negocio.Usuario CrearUsuarioN()
        {
            return new Negocio.Usuario(new FakeUsuarioRepository(), new EmpresaContextFake(IdEmpresa));
        }

        private static PermisosUsuarios Permiso(string consulta, string edicion, int diasEditar)
        {
            return new PermisosUsuarios
            {
                IdForm = 1,
                DiasPermitidosVer = 0,
                DiasPermitidosEditar = diasEditar,
                Formulario = new Formulario { FormConsulta = consulta, FormEdicion = edicion, FormEdicionExtra1 = "", FormEdicionExtra2 = "" }
            };
        }

        [Fact]
        public void PuedeEditarFormularioPorConsulta_PermisoDelPOS_NoHabilitaElControlFiscal()
        {
            var usuarioN = CrearUsuarioN();
            // "Pagos" y "POS - Ctas.Ctes Persona" comparten formAddOrEditPago como formulario de edicion.
            var soloPos = new Usuario { Id = 3, Permisos = new List<PermisosUsuarios>
            {
                Permiso("formCtaCtePersona", "formAddOrEditPago", 0),
                Permiso("formPagos", "formAddOrEditPago", -1),   // Pagos: solo ver (dias editar -1 = sin permiso de edicion)
            } };
            var conPagos = new Usuario { Id = 4, Permisos = new List<PermisosUsuarios>
            {
                Permiso("formPagos", "formAddOrEditPago", 0),
            } };
            var admin = new Usuario { Id = 5, Admin = true, Permisos = new List<PermisosUsuarios>() };

            Assert.False(usuarioN.PuedeEditarFormularioPorConsulta(soloPos, Permisos.Finanza.VerPagos));
            Assert.True(usuarioN.PuedeEditarFormularioPorConsulta(conPagos, Permisos.Finanza.VerPagos));
            Assert.True(usuarioN.PuedeEditarFormularioPorConsulta(admin, Permisos.Finanza.VerPagos));
            Assert.False(usuarioN.PuedeEditarFormularioPorConsulta(null, Permisos.Finanza.VerPagos));
        }
    }
}
