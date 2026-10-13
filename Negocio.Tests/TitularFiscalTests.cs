using System;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Titulares fiscales por empresa y por sucursal (2026-10-09, ver docs/DECISIONS.md): resolver emisor y punto de venta,
    // altas, habilitacion, asignacion a sucursales, sugerencia de titular RI, tipo de comprobante forzado y consumo.
    // Sin base de datos: repositorio en memoria.
    public class TitularFiscalTests
    {
        private const int IdEmpresa = 1;
        private const long CuitPrincipal = 20306210786;   // CUIT valido (digito verificador correcto)
        private const long CuitRI = 30500010912;          // CUIT valido de 11 digitos
        private const long CuitMono = 27230569040;

        private static Empresa Empresa() { return new Empresa { IdEmpresa = IdEmpresa, Cuit = CuitPrincipal, RazonSocialAfip = "Empresa SA", NombreFantasia = "La Carnicería", Domicilio = "Calle 1", Ciudad = "Rosario" }; }

        private static Sucursal Sucursal(int id = 10, int ptoVenta = 5, string direccion = "San Martín 100", string localidad = "Rosario")
        {
            return new Sucursal { IdSucursal = id, IdEmpresa = IdEmpresa, SucursalNombre = "Suc " + id, CodPuntoVentaAfip = ptoVenta, Direccion = direccion, Localidad = localidad };
        }

        private static (Negocio.TitularFiscal negocio, FakeTitularFiscalRepository repo, Entidades.TitularFiscal principal) CrearSut()
        {
            var repo = new FakeTitularFiscalRepository();
            var principal = repo.AgregarPrincipal(IdEmpresa, CuitPrincipal);
            return (new Negocio.TitularFiscal(repo), repo, principal);
        }

        // ------------------------------------------------------------------ resolver emisor

        [Fact]
        public void Resolver_SucursalSinAsignacion_UsaElPrincipalConElPuntoDeVentaDeLaSucursal()
        {
            var (negocio, _, principal) = CrearSut();

            var r = negocio.ResolverEmisor(Empresa(), Sucursal(ptoVenta: 7), null);

            Assert.True(r.Ok);
            Assert.Equal(principal.IdTitular, r.Emisor.IdTitular);
            Assert.Equal(CuitPrincipal, r.Emisor.Cuit);
            Assert.Equal(7, r.Emisor.PuntoVenta);
        }

        [Fact]
        public void Resolver_DosSucursalesConTitularesDistintos()
        {
            var (negocio, repo, principal) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.SucursalTitular[10] = principal.IdTitular;
            repo.SucursalTitular[11] = beto.IdTitular;

            var s1 = negocio.ResolverEmisor(Empresa(), Sucursal(10), null);
            var s2 = negocio.ResolverEmisor(Empresa(), Sucursal(11), null);

            Assert.Equal(CuitPrincipal, s1.Emisor.Cuit);
            Assert.Equal(CuitRI, s2.Emisor.Cuit);
            Assert.Equal(2, s2.Emisor.PuntoVenta);
            Assert.True(s2.Emisor.EsRRII);
        }

        [Fact]
        public void Resolver_TitularPedidoVaParaEsaFacturaYNoCambiaLaSucursal()
        {
            var (negocio, repo, principal) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.SucursalTitular[10] = principal.IdTitular;

            var pedido = negocio.ResolverEmisor(Empresa(), Sucursal(10), beto.IdTitular);
            var siguiente = negocio.ResolverEmisor(Empresa(), Sucursal(10), null);

            Assert.Equal(CuitRI, pedido.Emisor.Cuit);
            Assert.Equal(CuitPrincipal, siguiente.Emisor.Cuit);
            Assert.Equal(principal.IdTitular, repo.SucursalTitular[10]);
        }

        [Fact]
        public void Resolver_TitularNoHabilitadoOdeOtraEmpresa_SeRechaza()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            beto.Habilitado = false;
            var ajeno = repo.AgregarNoPrincipal(99, CuitMono, 3);

            var deshabilitado = negocio.ResolverEmisor(Empresa(), Sucursal(), beto.IdTitular);
            var deOtra = negocio.ResolverEmisor(Empresa(), Sucursal(), ajeno.IdTitular);
            var inexistente = negocio.ResolverEmisor(Empresa(), Sucursal(), 9999);

            Assert.False(deshabilitado.Ok);
            Assert.Equal("TITULAR_NO_DISPONIBLE", deshabilitado.Codigo);
            Assert.False(deOtra.Ok);
            Assert.False(inexistente.Ok);
        }

        [Fact]
        public void PuntoVenta_AsignadoGanaSobreElUnico_YElUnicoSirveSinAsignacion()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarPuntoVenta(beto.IdTitular, 2);

            Assert.Equal(2, negocio.ResolverEmisor(Empresa(), Sucursal(), beto.IdTitular).Emisor.PuntoVenta);

            repo.AgregarPuntoVenta(beto.IdTitular, 8);
            repo.GuardarPuntoVentaSucursal(beto.IdTitular, 10, 8);
            Assert.Equal(8, negocio.ResolverEmisor(Empresa(), Sucursal(10), beto.IdTitular).Emisor.PuntoVenta);
        }

        [Fact]
        public void PuntoVenta_TitularNoPrincipalSinPuntoResoluble_NoEstaDisponibleEnLaSucursal()
        {
            var (negocio, repo, _) = CrearSut();
            var sinLista = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            var variosSinAsignar = repo.AgregarNoPrincipal(IdEmpresa, CuitMono, 3);
            repo.AgregarPuntoVenta(variosSinAsignar.IdTitular, 3);
            repo.AgregarPuntoVenta(variosSinAsignar.IdTitular, 4);

            var a = negocio.ResolverEmisor(Empresa(), Sucursal(), sinLista.IdTitular);
            var b = negocio.ResolverEmisor(Empresa(), Sucursal(), variosSinAsignar.IdTitular);

            Assert.False(a.Ok);
            Assert.Equal("SIN_PUNTO_VENTA", a.Codigo);
            Assert.False(b.Ok);
            Assert.Equal("SIN_PUNTO_VENTA", b.Codigo);

            var opciones = negocio.OpcionesParaSucursal(Empresa(), Sucursal());
            Assert.False(opciones.First(o => o.IdTitular == sinLista.IdTitular).Disponible);
            Assert.True(opciones.First(o => o.EsPredeterminado).Disponible);
        }

        [Fact]
        public void PuntoVenta_Principal_UsaElDeLaSucursalAunqueSeaCero()
        {
            var (negocio, _, _) = CrearSut();

            var r = negocio.ResolverEmisor(Empresa(), Sucursal(ptoVenta: 0), null);

            Assert.True(r.Ok);
            Assert.Equal(0, r.Emisor.PuntoVenta);
        }

        // ------------------------------------------------------------------ tipo de comprobante

        [Theory]
        [InlineData(false, 2, 11)]   // titular no RI -> siempre C, aunque el cliente sea RI
        [InlineData(false, 3, 11)]
        [InlineData(false, 1, 11)]
        [InlineData(true, 2, 1)]     // titular RI + cliente RI -> A
        [InlineData(true, 3, 1)]     // titular RI + cliente monotributista -> A
        [InlineData(true, 1, 6)]     // titular RI + consumidor final -> B
        [InlineData(true, 4, 6)]     // titular RI + exento -> B
        public void TipoDeComprobante_SegunTitularYCliente(bool titularRI, int idIvaCliente, int esperado)
        {
            var factura = new FacturaElectronica();

            int tipo = factura.getCodTipoCbteAFIP(titularRI, Persona.RequiereFacturaA(idIvaCliente), false);

            Assert.Equal(esperado, tipo);
        }

        [Fact]
        public void NotaDeCredito_DeUnaFacturaC_EsNotaDeCreditoC()
        {
            Assert.Equal(FacturaElectronica.codNotaCreditoC_Afip, new FacturaElectronica().getCodTipoCbteAFIP(false, true, true));
        }

        // ------------------------------------------------------------------ sugerencia de titular RI

        [Fact]
        public void Sugerencia_ClienteRIConTitularMonotributo_OfreceLosTitularesRIDisponibles()
        {
            var (negocio, repo, principal) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            var emisor = negocio.ResolverEmisor(Empresa(), Sucursal(), null).Emisor;

            var lista = negocio.TitularesRIParaCliente(Empresa(), Sucursal(), emisor, 2);

            var o = Assert.Single(lista);
            Assert.Equal(beto.IdTitular, o.IdTitular);
        }

        [Fact]
        public void Sugerencia_SoloParaClienteRI_YSoloSiElEmisorNoEsRI()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            var emisorMono = negocio.ResolverEmisor(Empresa(), Sucursal(), null).Emisor;
            var emisorRI = negocio.ResolverEmisor(Empresa(), Sucursal(), beto.IdTitular).Emisor;

            Assert.Empty(negocio.TitularesRIParaCliente(Empresa(), Sucursal(), emisorMono, 3));   // cliente monotributista
            Assert.Empty(negocio.TitularesRIParaCliente(Empresa(), Sucursal(), emisorMono, 1));   // consumidor final
            Assert.Empty(negocio.TitularesRIParaCliente(Empresa(), Sucursal(), emisorRI, 2));     // el emisor ya es RI
        }

        [Fact]
        public void Sugerencia_SinTitularRIUtil_EstaVacia()
        {
            var (negocio, repo, _) = CrearSut();
            var deshabilitado = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarPuntoVenta(deshabilitado.IdTitular, 2);
            deshabilitado.Habilitado = false;
            var sinCertificado = repo.AgregarNoPrincipal(IdEmpresa, 27111111117, 2, "Sin cert", certificado: false);
            repo.AgregarPuntoVenta(sinCertificado.IdTitular, 3);
            var emisor = negocio.ResolverEmisor(Empresa(), Sucursal(), null).Emisor;

            Assert.Empty(negocio.TitularesRIParaCliente(Empresa(), Sucursal(), emisor, 2));
        }

        // ------------------------------------------------------------------ habilitar y asignar

        [Fact]
        public void SetHabilitado_NoSePuedeDeshabilitarElTitularDeUnaSucursal_NiElPrincipalConSucursalesSinAsignar()
        {
            var (negocio, repo, principal) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.NombresSucursal[10] = "Centro";
            repo.NombresSucursal[11] = "San Lorenzo";
            repo.SucursalTitular[11] = beto.IdTitular;     // la 10 no tiene asignacion: es del principal

            Assert.NotNull(negocio.SetHabilitado(IdEmpresa, beto.IdTitular, false));
            Assert.NotNull(negocio.SetHabilitado(IdEmpresa, principal.IdTitular, false));
            Assert.True(beto.Habilitado);

            repo.SucursalTitular[11] = principal.IdTitular;
            Assert.Null(negocio.SetHabilitado(IdEmpresa, beto.IdTitular, false));
            Assert.False(beto.Habilitado);
        }

        [Fact]
        public void Asignar_ExigeHabilitadoCertificadoYPuntoDeVentaDeLaLista()
        {
            var (negocio, repo, _) = CrearSut();
            var sinCert = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, certificado: false);
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitMono, 3, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.AgregarPuntoVenta(beto.IdTitular, 8);
            var suc = Sucursal(10);

            Assert.NotNull(negocio.AsignarTitularSucursal(suc, IdEmpresa, sinCert.IdTitular, null));
            Assert.NotNull(negocio.AsignarTitularSucursal(suc, IdEmpresa, beto.IdTitular, null));      // 2 puntos de venta y ninguno elegido
            Assert.NotNull(negocio.AsignarTitularSucursal(suc, IdEmpresa, beto.IdTitular, 99));        // fuera de la lista
            Assert.Null(negocio.AsignarTitularSucursal(suc, IdEmpresa, beto.IdTitular, 8));

            Assert.Equal(beto.IdTitular, repo.SucursalTitular[10]);
            Assert.Equal(8, repo.ObtenerPuntoVentaAsignado(beto.IdTitular, 10));
        }

        [Fact]
        public void Asignar_AlPrincipalNoPideNiGuardaPuntoDeVenta()
        {
            var (negocio, repo, principal) = CrearSut();

            Assert.Null(negocio.AsignarTitularSucursal(Sucursal(10), IdEmpresa, principal.IdTitular, null));

            Assert.Equal(principal.IdTitular, repo.SucursalTitular[10]);
            Assert.Empty(repo.Asignados);
        }

        [Fact]
        public void PuntoDeVenta_NoSeBorraNiSeDeshabilitaSiEstaAsignadoAUnaSucursal()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            var pv = repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.GuardarPuntoVentaSucursal(beto.IdTitular, 10, 2);

            Assert.NotNull(negocio.EliminarPuntoVenta(IdEmpresa, beto.IdTitular, 2));
            pv.Habilitado = false;
            Assert.NotNull(negocio.GuardarPuntoVenta(IdEmpresa, pv));

            repo.GuardarPuntoVentaSucursal(beto.IdTitular, 10, null);
            Assert.Null(negocio.EliminarPuntoVenta(IdEmpresa, beto.IdTitular, 2));
        }

        // ------------------------------------------------------------------ altas

        private static Entidades.TitularFiscal Nuevo(long cuit = CuitRI, int idIva = 2)
        {
            return new Entidades.TitularFiscal
            {
                IdEmpresa = IdEmpresa, IdPersona = 55, Cuit = cuit, RazonSocial = "Beto SA", IdIva = idIva,
                CondicionIVA = "Responsable Inscripto", InicioActividad = new DateTime(2019, 3, 1), Iibb = 901
            };
        }

        [Fact]
        public void Guardar_AltaValida_Funciona()
        {
            var (negocio, repo, _) = CrearSut();
            int id;

            string error = negocio.Guardar(Nuevo(), out id);

            Assert.Null(error);
            Assert.True(id > 0);
            Assert.NotNull(repo.Obtener(id));
        }

        [Fact]
        public void Guardar_RechazaDatosFiscalesIncompletosOInvalidos()
        {
            var (negocio, _, _) = CrearSut();
            int id;

            var sinInicio = Nuevo(); sinInicio.InicioActividad = null;
            var sinIibb = Nuevo(); sinIibb.Iibb = 0;
            var consumidorFinal = Nuevo(idIva: 1);
            var cuitMalo = Nuevo(cuit: 20306210787);   // digito verificador incorrecto
            var sinPersona = Nuevo(); sinPersona.IdPersona = null;

            Assert.NotNull(negocio.Guardar(sinInicio, out id));
            Assert.NotNull(negocio.Guardar(sinIibb, out id));
            Assert.NotNull(negocio.Guardar(consumidorFinal, out id));
            Assert.NotNull(negocio.Guardar(cuitMalo, out id));
            Assert.NotNull(negocio.Guardar(sinPersona, out id));
        }

        [Fact]
        public void Guardar_RechazaUnCuitQueLaPropiaEmpresaYaTiene()
        {
            var (negocio, repo, _) = CrearSut();
            repo.AgregarNoPrincipal(IdEmpresa, CuitMono, 3);
            int id;

            Assert.NotNull(negocio.Guardar(Nuevo(CuitPrincipal), out id));   // es el del principal (espejo de empresas.cuit)
            Assert.NotNull(negocio.Guardar(Nuevo(CuitMono), out id));        // ya es titular de esta empresa
        }

        // ------------------------------------------------------------------ titular compartido entre empresas (2026-10-12)

        [Fact]
        public void Guardar_PermiteUnCuitQueYaEsTitularOPrincipalEnOtraEmpresa()
        {
            var (negocio, repo, _) = CrearSut();
            repo.AgregarPrincipal(2, CuitRI, "Otra SA");            // CUIT de la empresa 2
            repo.AgregarNoPrincipal(3, CuitMono, 3, "Mono");        // titular de la empresa 3
            int id;

            Assert.Null(negocio.Guardar(Nuevo(CuitRI), out id));
            Assert.Null(negocio.Guardar(Nuevo(CuitMono), out id));
        }

        [Fact]
        public void ErrorSiFaltaConfirmarCompartido_SoloExigeConfirmacionSiElCuitEstaEnOtraEmpresa()
        {
            var (negocio, repo, _) = CrearSut();
            repo.AgregarPrincipal(2, CuitRI, "Otra SA");

            Assert.NotNull(negocio.ErrorSiFaltaConfirmarCompartido(CuitRI, IdEmpresa, false));
            Assert.Null(negocio.ErrorSiFaltaConfirmarCompartido(CuitRI, IdEmpresa, true));
            Assert.Null(negocio.ErrorSiFaltaConfirmarCompartido(CuitMono, IdEmpresa, false));   // no esta en ninguna otra
        }

        [Fact]
        public void OtrasEmpresasConCuit_NoCuentaLaPropiaNiLasRevocadas()
        {
            var (negocio, repo, _) = CrearSut();
            repo.AgregarNoPrincipal(2, CuitRI, 2);
            var revocado = repo.AgregarNoPrincipal(3, CuitRI, 2);
            revocado.Revocado = true;

            var otras = negocio.OtrasEmpresasConCuit(CuitRI, IdEmpresa);

            Assert.Single(otras);
            Assert.Equal(2, otras[0].IdEmpresa);
            Assert.Empty(negocio.OtrasEmpresasConCuit(CuitPrincipal, IdEmpresa));
        }

        [Fact]
        public void EmpresaAdministradora_EsDondeElCuitEsPrincipal_YSinPrincipalLaDelTitularMasAntiguo()
        {
            var (negocio, repo, _) = CrearSut();
            repo.AgregarNoPrincipal(2, CuitPrincipal, 3);      // la empresa 2 lo tiene como titular; la 1 como principal
            repo.AgregarNoPrincipal(3, CuitMono, 3);           // sin principal en ninguna empresa
            repo.AgregarNoPrincipal(4, CuitMono, 3);

            Assert.Equal(IdEmpresa, negocio.EmpresaAdministradoraDeCuit(CuitPrincipal));
            Assert.True(negocio.PuedeAdministrarCertificado(CuitPrincipal, IdEmpresa));
            Assert.False(negocio.PuedeAdministrarCertificado(CuitPrincipal, 2));
            Assert.Equal(3, negocio.EmpresaAdministradoraDeCuit(CuitMono));
            Assert.False(negocio.PuedeAdministrarCertificado(CuitMono, 4));
            Assert.True(negocio.PuedeAdministrarCertificado(CuitRI, 4));        // nadie lo administra todavia: primer alta
        }

        [Fact]
        public void OtrasEmpresasConPuntoVenta_AvisaSoloDelMismoCuitYPuntoDeVenta()
        {
            var (negocio, repo, _) = CrearSut();
            var enOtra = repo.AgregarNoPrincipal(2, CuitRI, 2);
            repo.AgregarPuntoVenta(enOtra.IdTitular, 3);
            var distintoCuit = repo.AgregarNoPrincipal(3, CuitMono, 3);
            repo.AgregarPuntoVenta(distintoCuit.IdTitular, 3);

            var avisos = negocio.OtrasEmpresasConPuntoVenta(CuitRI, 3, IdEmpresa);

            Assert.Single(avisos);
            Assert.Equal(2, avisos[0].IdEmpresa);
            Assert.Empty(negocio.OtrasEmpresasConPuntoVenta(CuitRI, 4, IdEmpresa));
        }

        [Fact]
        public void ClaveCuit_SeGuardaPorCuitYEntorno_Compartida()
        {
            var (negocio, _, _) = CrearSut();

            negocio.GuardarClaveCuit(CuitRI, false, "certificado.pfx", "cifrada-prod");

            Assert.Equal("cifrada-prod", negocio.ObtenerClaveCuit(CuitRI, false));
            Assert.Null(negocio.ObtenerClaveCuit(CuitRI, true));
            Assert.Null(negocio.ObtenerClaveCuit(CuitMono, false));
        }

        // ------------------------------------------------------------------ revocar la titularidad (super-admin)

        [Fact]
        public void Revocar_SiNuncaFacturo_BorraElTitularYLasSucursalesVuelvenAlPrincipal()
        {
            var (negocio, repo, principal) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.AsignarTitularSucursal(10, beto.IdTitular, 2);

            var r = negocio.Revocar(IdEmpresa, beto.IdTitular, 99);

            Assert.True(r.Ok);
            Assert.True(r.Borrado);
            Assert.Null(repo.Obtener(beto.IdTitular));
            Assert.DoesNotContain(negocio.Listar(IdEmpresa, false), t => t.IdTitular == beto.IdTitular);
            Assert.Null(repo.ObtenerIdTitularDeSucursal(10));
            Assert.Empty(repo.ListarPuntosVenta(beto.IdTitular));
            Assert.Equal(principal.IdTitular, negocio.ResolverEmisor(Empresa(), Sucursal(10), null).Emisor.IdTitular);
        }

        [Fact]
        public void Revocar_SiYaFacturo_QuedaMarcadoYNoSePuedeHabilitarNiEmitir()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            repo.TitularesConFacturas.Add(beto.IdTitular);

            var r = negocio.Revocar(IdEmpresa, beto.IdTitular, 99);

            Assert.True(r.Ok);
            Assert.False(r.Borrado);
            Assert.True(repo.Obtener(beto.IdTitular).Revocado);
            Assert.NotNull(negocio.SetHabilitado(IdEmpresa, beto.IdTitular, true));
            var emision = negocio.ResolverEmisor(Empresa(), Sucursal(), beto.IdTitular);
            Assert.False(emision.Ok);
            Assert.Contains("cancelada", emision.Mensaje);
            Assert.DoesNotContain(negocio.OpcionesParaSucursal(Empresa(), Sucursal()), o => o.IdTitular == beto.IdTitular);
        }

        [Fact]
        public void Revocar_NoPermiteCancelarAlPrincipalNiAUnTitularAjeno()
        {
            var (negocio, repo, principal) = CrearSut();
            var deOtra = repo.AgregarNoPrincipal(2, CuitRI, 2);

            Assert.False(negocio.Revocar(IdEmpresa, principal.IdTitular, 99).Ok);
            Assert.False(negocio.Revocar(IdEmpresa, deOtra.IdTitular, 99).Ok);
            Assert.False(negocio.Revocar(IdEmpresa, 12345, 99).Ok);
        }

        [Fact]
        public void EmisorDeFactura_TitularRevocado_MarcaRevocadoParaBloquearLaNotaDeCredito()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.TitularesConFacturas.Add(beto.IdTitular);
            negocio.Revocar(IdEmpresa, beto.IdTitular, 99);
            var factura = new FacturaElectronica { IdTitular = beto.IdTitular, CuitEmisor = CuitRI, PtoVtaAfip = "2" };

            var emisor = negocio.EmisorDeFactura(factura, Empresa(), Sucursal());

            Assert.True(emisor.Revocado);
        }

        [Fact]
        public void Reponer_QuitaLaCancelacionYQuedaDeshabilitado()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.TitularesConFacturas.Add(beto.IdTitular);
            negocio.Revocar(IdEmpresa, beto.IdTitular, 99);

            Assert.Null(negocio.Reponer(IdEmpresa, beto.IdTitular));

            Assert.False(repo.Obtener(beto.IdTitular).Revocado);
            Assert.False(repo.Obtener(beto.IdTitular).Habilitado);
            Assert.Null(negocio.SetHabilitado(IdEmpresa, beto.IdTitular, true));
            Assert.NotNull(negocio.Reponer(IdEmpresa, beto.IdTitular));   // ya no estaba cancelado
        }

        [Fact]
        public void Guardar_UnTitularRevocadoNoSeEdita()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.TitularesConFacturas.Add(beto.IdTitular);
            negocio.Revocar(IdEmpresa, beto.IdTitular, 99);
            var edicion = Nuevo(CuitRI);
            edicion.IdTitular = beto.IdTitular;
            int id;

            Assert.NotNull(negocio.Guardar(edicion, out id));
        }

        [Fact]
        public void Guardar_ElCuitDeUnTitularNoSeCambia()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            var cambio = Nuevo(CuitMono);
            cambio.IdTitular = beto.IdTitular;
            int id;

            Assert.NotNull(negocio.Guardar(cambio, out id));
        }

        [Fact]
        public void Guardar_DelPrincipal_SoloCambiaTopesYVentana()
        {
            var (negocio, repo, principal) = CrearSut();
            var edicion = new Entidades.TitularFiscal
            {
                IdTitular = principal.IdTitular, IdEmpresa = IdEmpresa, RazonSocial = "OTRA", Cuit = 1,
                LimiteMensual = 100m, LimiteAnual = 1200m, VentanaAnual = Entidades.TitularFiscal.VentanaCalendario
            };
            int id;

            Assert.Null(negocio.Guardar(edicion, out id));

            Assert.Equal(CuitPrincipal, principal.Cuit);
            Assert.Equal("Empresa SA", principal.RazonSocial);
            Assert.Equal(1200m, principal.LimiteAnual);
            Assert.Equal(Entidades.TitularFiscal.VentanaCalendario, principal.VentanaAnual);
        }

        [Fact]
        public void Guardar_TopesNegativosOVentanaInvalida_SeRechazan()
        {
            var (negocio, _, _) = CrearSut();
            int id;
            var negativo = Nuevo(); negativo.LimiteMensual = -1m;
            var ventana = Nuevo(); ventana.VentanaAnual = "OTRA";

            Assert.NotNull(negocio.Guardar(negativo, out id));
            Assert.NotNull(negocio.Guardar(ventana, out id));
        }

        // ------------------------------------------------------------------ emisor de una factura emitida

        [Fact]
        public void EmisorDeFactura_ConDatosGuardados_UsaLosDelComprobanteYSuPuntoDeVenta()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            var factura = new FacturaElectronica
            {
                IdTitular = beto.IdTitular, CuitEmisor = CuitRI, RazonSocialEmisor = "Beto", CondicionIvaEmisor = "Responsable Inscripto",
                EsRRIIEmisor = true, IibbEmisor = 123, InicioActividadEmisor = new DateTime(2020, 1, 1), DomicilioEmisor = "Belgrano 5 - Rosario",
                PtoVtaAfip = "00003"
            };

            // La sucursal ya paso a otro titular y a otro punto de venta: el comprobante conserva el suyo.
            var emisor = negocio.EmisorDeFactura(factura, Empresa(), Sucursal(ptoVenta: 9));

            Assert.Equal(CuitRI, emisor.Cuit);
            Assert.Equal(3, emisor.PuntoVenta);
            Assert.True(emisor.EsRRII);
            Assert.Equal("Belgrano 5 - Rosario", emisor.Domicilio);
            Assert.Equal("certificado.pfx", emisor.NombreCertificadoPfx);
        }

        [Fact]
        public void EmisorDeFactura_ElTitularDeshabilitadoSigueSirviendoParaUnaNotaDeCredito()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            beto.Habilitado = false;
            var factura = new FacturaElectronica { IdTitular = beto.IdTitular, CuitEmisor = CuitRI, PtoVtaAfip = "2" };

            var emisor = negocio.EmisorDeFactura(factura, Empresa(), Sucursal());

            Assert.Equal(CuitRI, emisor.Cuit);
            Assert.Equal(beto.IdTitular, emisor.IdTitular);
        }

        [Fact]
        public void EmisorDeFactura_SinDatosGuardados_ImprimeComoSiempre()
        {
            var (negocio, _, _) = CrearSut();
            var empresa = Empresa();
            empresa.NombreCertificado_pfx = "certif-prod.pfx";
            var factura = new FacturaElectronica { PtoVtaAfip = "00004" };

            var emisor = negocio.EmisorDeFactura(factura, empresa, Sucursal(ptoVenta: 9));

            Assert.True(emisor.EsLegado);
            Assert.Equal(CuitPrincipal, emisor.Cuit);
            Assert.Equal(4, emisor.PuntoVenta);                         // el del comprobante, no el actual de la sucursal
            Assert.Equal("Calle 1 - Rosario", emisor.Domicilio);        // PDF y ticket: domicilio de la empresa
            Assert.Equal("San Martín 100", emisor.DomicilioModal);      // modal: direccion de la sucursal
            Assert.Equal("certif-prod.pfx", emisor.NombreCertificadoPfx);
        }

        [Fact]
        public void EmisorFiscal_CopiarA_GuardaTodoYSeLeeDeVuelta()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2, "Beto");
            repo.AgregarPuntoVenta(beto.IdTitular, 2);
            var emisor = negocio.ResolverEmisor(Empresa(), Sucursal(), beto.IdTitular).Emisor;
            var factura = new FacturaElectronica { PtoVtaAfip = "00002" };

            emisor.CopiarA(factura);
            var releido = Entidades.EmisorFiscal.DesdeFactura(factura, Empresa(), Sucursal());

            Assert.True(factura.TieneEmisorGuardado);
            Assert.Equal(beto.IdTitular, factura.IdTitular);
            Assert.Equal(CuitRI, releido.Cuit);
            Assert.Equal("Beto", releido.RazonSocial);
            Assert.True(releido.EsRRII);
            Assert.Equal(2, releido.PuntoVenta);
        }

        // ------------------------------------------------------------------ texto de "Cond. IVA"

        [Theory]
        [InlineData(2, "Responsable Inscripto")]
        [InlineData(3, "Responsable Monotributo")]
        public void CondicionIva_DesdeTitular_SeDerivaDeIdIvaYIgnoraElTextoLibre(int idIva, string esperado)
        {
            // Texto libre vacio (monotributista dado de alta desde una persona) o "RRII" tipeado a mano: da igual.
            foreach (string textoLibre in new[] { null, "", "RRII" })
            {
                var titular = new TitularFiscal { IdTitular = 1, IdIva = idIva, CondicionIVA = textoLibre };

                var emisor = EmisorFiscal.DesdeTitular(titular, Empresa(), Sucursal(), 1);

                Assert.Equal(esperado, emisor.CondicionIVA);
            }
        }

        [Fact]
        public void CondicionIva_DesdeTitular_ConOtroCodigoUsaElTextoDeRespaldo()
        {
            var titular = new TitularFiscal { IdTitular = 1, IdIva = 4, CondicionIVA = "Exento" };

            Assert.Equal("Exento", EmisorFiscal.DesdeTitular(titular, Empresa(), Sucursal(), 1).CondicionIVA);
        }

        [Fact]
        public void CondicionIva_Legado_SeDerivaDelFlagEsRRIIDeLaEmpresa()
        {
            var empresa = Empresa();
            empresa.CondicionIVA = "";
            empresa.EsRRII = false;
            Assert.Equal("Responsable Monotributo", EmisorFiscal.Legado(empresa, Sucursal()).CondicionIVA);

            empresa.CondicionIVA = "RRII";
            empresa.EsRRII = true;
            Assert.Equal("Responsable Inscripto", EmisorFiscal.Legado(empresa, Sucursal()).CondicionIVA);
        }

        [Fact]
        public void CondicionIva_DesdeFactura_RespetaElTextoGuardadoYDerivaSiQuedoVacio()
        {
            var conTexto = new FacturaElectronica
            {
                IdTitular = 1, CuitEmisor = CuitRI, CondicionIvaEmisor = "Responsable Inscripto", EsRRIIEmisor = true, PtoVtaAfip = "1"
            };
            var sinTexto = new FacturaElectronica
            {
                IdTitular = 1, CuitEmisor = CuitRI, CondicionIvaEmisor = "", EsRRIIEmisor = false, PtoVtaAfip = "1"
            };

            Assert.Equal("Responsable Inscripto", EmisorFiscal.DesdeFactura(conTexto, Empresa(), Sucursal()).CondicionIVA);
            Assert.Equal("Responsable Monotributo", EmisorFiscal.DesdeFactura(sinTexto, Empresa(), Sucursal()).CondicionIVA);
        }

        // ------------------------------------------------------------------ domicilio

        [Fact]
        public void Domicilio_EsLaDireccionDeLaSucursal_ConRespaldoALaEmpresa()
        {
            Assert.Equal("San Martín 100 - Rosario", EmisorFiscal.DomicilioNuevo(Sucursal(), Empresa()));
            Assert.Equal("San Martín 100", EmisorFiscal.DomicilioNuevo(Sucursal(localidad: ""), Empresa()));
            Assert.Equal("Calle 1 - Rosario", EmisorFiscal.DomicilioNuevo(Sucursal(direccion: "", localidad: ""), Empresa()));
        }

        // ------------------------------------------------------------------ facturado y topes

        [Fact]
        public void Rangos_MensualYAnual()
        {
            DateTime d, h;
            Negocio.TitularFiscal.RangoMensual(2026, 2, out d, out h);
            Assert.Equal(new DateTime(2026, 2, 1), d);
            Assert.Equal(new DateTime(2026, 2, 28), h);

            Negocio.TitularFiscal.RangoAnual(Entidades.TitularFiscal.VentanaMovil12Meses, 2026, 3, out d, out h);
            Assert.Equal(new DateTime(2025, 4, 1), d);       // los 12 meses que terminan en marzo 2026
            Assert.Equal(new DateTime(2026, 3, 31), h);

            Negocio.TitularFiscal.RangoAnual(Entidades.TitularFiscal.VentanaCalendario, 2026, 3, out d, out h);
            Assert.Equal(new DateTime(2026, 1, 1), d);
            Assert.Equal(new DateTime(2026, 3, 31), h);
        }

        [Fact]
        public void Consumo_MesYVentanaAnualPorTitular()
        {
            var (negocio, repo, principal) = CrearSut();
            principal.LimiteMensual = 1000m;
            principal.LimiteAnual = 10000m;
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            beto.VentanaAnual = Entidades.TitularFiscal.VentanaCalendario;
            repo.Facturas[principal.IdTitular + "|2026-02"] = 400m;
            repo.Facturas[principal.IdTitular + "|2025-06"] = 3000m;   // dentro de los 12 meses moviles, no del calendario
            repo.Facturas[beto.IdTitular + "|2026-02"] = 50m;
            repo.Facturas[beto.IdTitular + "|2025-06"] = 3000m;

            var consumo = negocio.ConsumoDeTitulares(IdEmpresa, 2026, 2);

            var cp = consumo.First(c => c.Titular.IdTitular == principal.IdTitular);
            Assert.Equal(400m, cp.FacturadoMes);
            Assert.Equal(3400m, cp.FacturadoAnual);
            Assert.Equal(40m, cp.PorcentajeMensual);
            Assert.Equal(34m, cp.PorcentajeAnual);

            var cb = consumo.First(c => c.Titular.IdTitular == beto.IdTitular);
            Assert.Equal(50m, cb.FacturadoAnual);        // calendario: solo 2026
            Assert.Null(cb.PorcentajeMensual);           // sin tope cargado: solo el monto
        }

        [Fact]
        public void Consumo_CuitCompartido_ElTopeSeCompararContraElTotalDelCuitEnTodasLasEmpresas()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            beto.LimiteMensual = 1000m;
            beto.LimiteAnual = 10000m;
            var enOtra = repo.AgregarNoPrincipal(2, CuitRI, 2);          // el mismo CUIT en la empresa 2
            var enOtraMas = repo.AgregarNoPrincipal(3, CuitRI, 2);       // y en la 3
            repo.Facturas[beto.IdTitular + "|2026-02"] = 100m;
            repo.Facturas[enOtra.IdTitular + "|2026-02"] = 250m;
            repo.Facturas[enOtraMas.IdTitular + "|2026-02"] = 150m;

            var cb = negocio.ConsumoDeTitulares(IdEmpresa, 2026, 2).First(c => c.Titular.IdTitular == beto.IdTitular);

            Assert.Equal(100m, cb.FacturadoMes);                 // lo de esta empresa se sigue mostrando
            Assert.Equal(500m, cb.TotalCuitMes);                 // total del CUIT en las tres
            Assert.Equal(500m, cb.TotalCuitAnual);
            Assert.Equal(3, cb.PorEmpresaMes.Count);             // desglose (lo ve solo el superadmin en pantalla)
            Assert.Equal(50m, cb.PorcentajeMensual);             // 500 sobre el tope mensual de 1000
            Assert.Equal(5m, cb.PorcentajeAnual);                // 500 sobre el tope anual de 10000
        }

        [Fact]
        public void Consumo_CuitSoloEnEstaEmpresa_NoCalculaTotalYUsaLoDeEstaEmpresa()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            beto.LimiteMensual = 1000m;
            repo.Facturas[beto.IdTitular + "|2026-02"] = 100m;

            var cb = negocio.ConsumoDeTitulares(IdEmpresa, 2026, 2).First(c => c.Titular.IdTitular == beto.IdTitular);

            Assert.Null(cb.TotalCuitMes);
            Assert.Null(cb.TotalCuitAnual);
            Assert.Empty(cb.PorEmpresaMes);
            Assert.Equal(10m, cb.PorcentajeMensual);
        }

        [Fact]
        public void Consumo_CuitCompartido_LoFacturadoEnUnaEmpresaConLaTitularidadCanceladaSigueSumando()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            var enOtra = repo.AgregarNoPrincipal(2, CuitRI, 2);
            enOtra.Revocado = true;                                 // cancelada, pero la persona facturo ahi igual
            repo.Facturas[beto.IdTitular + "|2026-02"] = 100m;
            repo.Facturas[enOtra.IdTitular + "|2026-02"] = 50m;

            var cb = negocio.ConsumoDeTitulares(IdEmpresa, 2026, 2).First(c => c.Titular.IdTitular == beto.IdTitular);

            Assert.Equal(150m, cb.TotalCuitMes);
        }

        [Fact]
        public void Consumo_UnTitularCancelado_NoSeComparaConTopesDeTodoElCuit()
        {
            var (negocio, repo, _) = CrearSut();
            var beto = repo.AgregarNoPrincipal(IdEmpresa, CuitRI, 2);
            repo.AgregarNoPrincipal(2, CuitRI, 2);
            beto.Revocado = true;

            var cb = negocio.ConsumoDeTitulares(IdEmpresa, 2026, 2).First(c => c.Titular.IdTitular == beto.IdTitular);

            Assert.Null(cb.TotalCuitMes);
        }

        // ------------------------------------------------------------------ CUIT

        [Theory]
        [InlineData("20306210786", true)]
        [InlineData("20-30621078-6", true)]
        [InlineData("20306210787", false)]
        [InlineData("2030621078", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Cuit_Validacion(string cuit, bool esperado)
        {
            Assert.Equal(esperado, CuitHelper.EsValido(cuit));
        }

        [Fact]
        public void Cuit_NormalizaYParseaSoloSiEsValido()
        {
            Assert.Equal("20306210786", CuitHelper.Normalizar("20-30621078-6"));
            Assert.Equal(20306210786L, CuitHelper.ParsearValido("20-30621078-6"));
            Assert.Equal(0L, CuitHelper.ParsearValido("20-30621078-7"));
        }
    }
}
